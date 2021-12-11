using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KalosfideAPI.Clients
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class ClientController : AvecCarteController
    {
        private readonly IUtileService _utile;
        private readonly IClientService _service;

        public ClientController(IClientService service,
            IUtileService utile,
            IUtilisateurService utilisateurService
            ) : base(utilisateurService)
        {
            _utile = utile;
            _service = service;
        }

        [HttpPost("/api/client/ajoute")]
        [ProducesResponseType(201)] // Created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute([FromQuery] ClientVueAjoute vue)
        {
            // seul le fournisseur du site peut créer un client
            // la key de la vue est celle du site
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(vue);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Role client = await _service.ClientDeNom(vue, vue.Nom);
            if (client != null)
            {
                return RésultatBadRequest("nom", "nomPris");
            }

            // Crée un utilisateur sans ApplicationUser
            RetourDeService<Utilisateur> retourUtilisateur = await UtilisateurService.CréeUtilisateur();
            if (!retourUtilisateur.Ok)
            {
                return SaveChangesActionResult(retourUtilisateur);
            }

            // le Role doit être créé dans l'état Actif
            vue.Etat = TypeEtatRole.Actif;
            // Crée le Role
            RetourDeService<Role> retour = await _service.Ajoute(retourUtilisateur.Entité, vue);
            if (!retour.Ok)
            {
                // La création du Role a échoué, il faut supprimer l'Utilisateur
                await UtilisateurService.Supprime(retourUtilisateur.Entité);
                return SaveChangesActionResult(retour);
            }

            return StatusCode(201, retour.Entité.KeyParam);
        }

        [HttpPut("/api/client/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(ClientVue vue)
        {
            CarteUtilisateur carte = await CréeCarteClientOuFournisseurDeClient(vue);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            if (Role.EstFournisseur(carte.Role))
            {
                if (await _service.Email(vue) != null)
                {
                    // l'utilisateur est le fournisseur du site mais le client gére son compte
                    return RésultatInterdit("Vous ne pouvez pas modifier un client qui gére son compte.");
                }
            }

            Role donnée = await _service.Lit(vue);
            if (donnée == null)
            {
                return NotFound();
            }

            Role client = await _service.ClientDeNom(carte.Role.Site, vue.Nom);
            if (client != null && client.Uid != vue.Uid)
            {
                return RésultatBadRequest("nom", "nomPris");
            }

            RetourDeService<Role> retour = await _service.Edite(donnée, ClientVue.RoleVue(vue));

            return SaveChangesActionResult(retour);
        }

        private async Task<IActionResult> Etat(KeyUidRno keyClient, string état)
        {
            // on lit dans le bdd le Role avec Site et Utilisateur et éventuellement ApplicationUser
            Role client = await _service.LitRole(keyClient.Uid, keyClient.Rno);
            if (client == null)
            {
                // le compte n'existe pas
                return NotFound();
            }
            if (Role.EstFournisseur(client))
            {
                // c'est le fournisseur
                return RésultatBadRequest("Le role est celui d'un fournisseur");
            }

            // seul le fournisseur peut changer l'état d'un client
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(client.Site);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            if (état == TypeEtatRole.Actif)
            {
                // on ne peut pas désactiver un client d'Etat déjà Actif
                if (client.Etat == TypeEtatRole.Actif)
                {
                    return BadRequest();
                }
                // le fournisseur peut activer un client Nouveau et réactiver un client Inactif ou Fermé
                return SaveChangesActionResult(await _service.Active(client));
            }
            else
            {
                if (client.Etat == TypeEtatRole.Inactif || client.Etat == TypeEtatRole.Fermé)
                {
                    return BadRequest();
                }

                // les demandes de passer à l'un des Etats Inactif ou Fermé sont traités de la même façon
                if (client.Etat == TypeEtatRole.Nouveau)
                {
                    // toutes les modifications apportées à la bdd par l'utilisateur qui gère ce compte sont supprimées
                    return SaveChangesActionResult(await _service.Supprime(client));
                }
                else
                {
                    // Si le Role a été créé par le fournisseur et s'il y a des documents avec des lignes, change son Etat en Fermé  il est supprimé sinon.
                    // Si le Role a été créé par le fournisseur et est vide, supprime le Role.
                    // Si le Role a été créé en répondant à une invitation, change son Etat en Inactif et il passera automatiquement à l'Etat Fermé
                    // quand le client se connectera ou quand le fournisseur chargera la liste des clients aprés 60 jours.
                    return SaveChangesActionResult(await _service.Inactive(client));
                }
            }
        }

        [HttpPut("/api/client/active")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Active([FromQuery] KeyUidRno keyClient)
        {
            return await Etat(keyClient, TypeEtatRole.Actif);
        }

        [HttpPut("/api/client/inactive")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Inactive([FromQuery] KeyUidRno keyClient)
        {
            return await Etat(keyClient, TypeEtatRole.Inactif);
        }

        [HttpGet("/api/client/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRno keyClient)
        {
            CarteUtilisateur carte = await CréeCarteClientOuFournisseurDeClient(keyClient);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok(carte.Role);
        }

        [HttpGet("/api/client/liste")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            List<ClientEtatVue> clients = await _service.ClientsDuSite(keySite);
            List<InvitationVue> invitations = await UtilisateurService.Invitations(keySite);
            return Ok(new
            {
                Clients = clients,
                Invitations = invitations,
                Date = DateTime.Now
            });
        }
    }

}
