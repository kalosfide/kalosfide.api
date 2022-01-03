using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Data;
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
    public class ClientController : AvecIdUintController<Client, ClientAAjouter, ClientAEditer>
    {
        private readonly IUtileService _utile;

        public ClientController(IClientService service,
            IUtileService utile,
            IUtilisateurService utilisateurService
            ) : base(service, utilisateurService)
        {
            _utile = utile;
        }

        private IClientService _service { get => __service as IClientService; }

        /// <summary>
        /// Ajoute un Client sans Utilisateur à la table des Clients.
        /// </summary>
        /// <param name="ajout">ClientAAjouter contenant l'Id du Site et les données de Client sans Etat</param>
        /// <returns></returns>
        [HttpPost("/api/client/ajoute")]
        [ProducesResponseType(201)] // Created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Ajoute([FromQuery] ClientAAjouter ajout)
        {
            // seul le fournisseur du site peut créer un client
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(ajout.SiteId, EtatsRolePermis.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesData(ajout, Client.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return await base.Ajoute(ajout);
        }

        [HttpPut("/api/client/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(ClientAEditer vue)
        {
            Client donnée = await _service.Lit(vue.Id);
            if (donnée == null)
            {
                return NotFound();
            }

            CarteUtilisateur carte = await CréeCarteClientOuFournisseurDeClient(vue.Id, EtatsRolePermis.Actif, EtatsRolePermis.PasInactif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesDataAnnulable(vue, Client.AvérifierSansEspacesDataAnnulable);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (carte.Fournisseur != null)
            {
                if (donnée.UtilisateurId != null)
                {
                    // l'utilisateur est le fournisseur du site mais le client gère son compte
                    carte.Erreur = RésultatInterdit("Vous ne pouvez pas modifier un client qui gère son compte.");
                }
            }
            return await Edite(donnée, vue);
        }

        private async Task<IActionResult> Etat(uint idClient, EtatRole état)
        {
            Client client = await _service.Lit(idClient);
            if (client == null)
            {
                // le compte n'existe pas
                return NotFound();
            }

            // seul le fournisseur actif peut changer l'état d'un client
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(client.Site.Id, EtatsRolePermis.Actif);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            if (état == EtatRole.Actif)
            {
                // on ne peut pas désactiver un client d'Etat déjà Actif
                if (client.Etat == EtatRole.Actif)
                {
                    return BadRequest();
                }
                // le fournisseur peut activer un client Nouveau et réactiver un client Inactif ou Fermé
                return SaveChangesActionResult(await _service.Active(client));
            }
            else
            {
                if (client.Etat == EtatRole.Inactif || client.Etat == EtatRole.Fermé)
                {
                    return BadRequest();
                }

                // les demandes de passer à l'un des Etats Inactif ou Fermé sont traités de la même façon
                if (client.Etat == EtatRole.Nouveau)
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
        public async Task<IActionResult> Active([FromQuery] uint idClient)
        {
            return await Etat(idClient, EtatRole.Actif);
        }

        [HttpPut("/api/client/inactive")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Inactive([FromQuery] uint idClient)
        {
            return await Etat(idClient, EtatRole.Inactif);
        }

        [HttpGet("/api/client/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] uint idClient)
        {
            Client client = await _service.Lit(idClient);
            if (client == null)
            {
                // le compte n'existe pas
                return NotFound();
            }

            CarteUtilisateur carte = await CréeCarteClientOuFournisseurDeClient(idClient, EtatsRolePermis.Actif, EtatsRolePermis.PasInactif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok(client);
        }

        [HttpGet("/api/client/liste")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste([FromQuery] uint idSite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(idSite, EtatsRolePermis.Actif);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            // Array des EtatUtilisateur du client permis
            EtatUtilisateur[] étatsUtilisateurPermis = new EtatUtilisateur[] { EtatUtilisateur.Nouveau, EtatUtilisateur.Actif };
            List<ClientEtatVue> clients = await _service.ClientsDuSite(idSite, EtatsRolePermis.Actif, étatsUtilisateurPermis);
            List<InvitationVue> invitations = await _service.InvitationsSansId(idSite);
            return Ok(new
            {
                Clients = clients,
                Invitations = invitations,
                Date = DateTime.Now
            });
        }

        /// <summary>
        /// Enregistre l'invitation postée et envoie un message email contenant un lien avec l'invitation codée.
        /// S'il existe déjà une Invitation enregistrée avec la même Id (du Fournisseur) et le même Email, elle est mise à jour.
        /// </summary>
        /// <param name="invitation">Invitation</param>
        /// <returns></returns>
        [HttpPost("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> Invitation([FromBody] Invitation invitation)
        {
            // l'invitation a le même Id que le Fournisseur qui l'envoie
            CarteUtilisateur carte = await CréeCarteFournisseur(invitation.Id, EtatsRolePermis.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Invitation enregistrée = null;
            Client client = null;
            // Si l'invitation invite à prendre en charge un Client, il faut vérifier ce client
            if (invitation.ClientId != null)
            {
                client = await _service.Lit(invitation.ClientId.Value);
                // Vérifie que le Client existe
                if (client == null)
                {
                    return NotFound();
                }
                // Vérifie que le Client n'a pas d'Utilisateur
                if (client.UtilisateurId != null)
                {
                    return RésultatBadRequest("Le client est déjà attribué à un utilisateur.");
                }
                // Vérifie qu'il n'y a pas d'Invitation à prendre en charge ce Client
                enregistrée = await _service.InvitationDeClientId(invitation.ClientId.Value);
                if (enregistrée != null)
                {
                    if (enregistrée.Email != invitation.Email)
                    {
                        return RésultatBadRequest("Une invitation à prendre en charge ce client a déjà été envoyée.");
                    }
                }
            }

            // Vérifie que l'utilisateur invité n'est pas le Fournisseur du site
            if (carte.Utilisateur.Email==invitation.Email)
            {
                return RésultatBadRequest("L'utilisateur invité est le Fournisseur du site.");
            }
            // Vérifie que l'utilisateur invité n'est pas déjà client du site
            if (await _service.ClientDeEmail(invitation.Id, invitation.Email) != null)
            {
                return RésultatBadRequest("L'utilisateur invité est déjà client du site.");
            }

            // l'invitation est Ok
            invitation.Date = DateTime.Now;

            // envoie l'invitation
            await _service.EnvoieEmailInvitation(invitation, carte.Fournisseur, client);

            // Engistre l'envoi
            if (enregistrée == null)
            {
                enregistrée = await _service.LitInvitation(invitation);
            }
            RetourDeService retour = await _service.EnregistreInvitation(invitation, enregistrée);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return RésultatCréé(invitation);
        }

        /// <summary>
        /// Retourne les informations sur le Fournisseur et éventuellement le client à prendre en charge si le code correspond à une invitation enregistrée
        /// pour pouvoir les afficher dans la page Devenir client
        /// </summary>
        /// <param name="code">code du lien du message email d'invitation</param>
        /// <returns></returns>
        [HttpGet("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Invitation([FromQuery] string code)
        {

            Invitation invitation = _service.DécodeInvitation(code);
            if (invitation == null)
            {
                return RésultatBadRequest("Pas d'invitation");
            }

            Invitation enregistrée = await _service.InvitationEnregistrée(invitation);
            // il doit y avoir une invitation enregistrée identique à l'invitation décodée
            if (enregistrée == null)
            {
                return NotFound();
            }

            InvitationContexte contexte = new InvitationContexte
            {
                Fournisseur = enregistrée.Fournisseur,
                Client = enregistrée.Client
            };
            return Ok(contexte);
        }

        [HttpDelete("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> AnnuleInvitation([FromQuery] InvitationKey key)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(key.Id, EtatsRolePermis.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Invitation invitation = await _service.LitInvitation(key);
            if (invitation == null)
            {
                return NotFound();
            }

            RetourDeService retour = await _service.SupprimeInvitation(invitation);
            return SaveChangesActionResult(retour);
        }


        [HttpPost("nouveau")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Nouveau([FromBody] DevenirClientVue vue)
        {
            // Vérifie que Nom et Adresse sont présents et non vides
            VérifieSansEspacesData(vue, Client.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifie que l'invitation est présente et valide
            IActionResult absentOuInvalide = RésultatBadRequest("Invitation absente ou invalide");
            if (vue.Code == null)
            {
                return absentOuInvalide;
            }
            Invitation invitation = _service.DécodeInvitation(vue.Code);
            if (invitation == null)
            {
                return absentOuInvalide;
            }
            if (vue.Email != invitation.Email)
            {
                // l'email de la réponse n'est pas celui de l'invitation décodée
                return absentOuInvalide;
            }

            // Vérifie qu'il y a une invitation enregistrée identique à l'invitation de la vue
            Invitation enregistrée = await _service.InvitationEnregistrée(invitation);
            if (enregistrée == null)
            {
                return absentOuInvalide;
            }

            Utilisateur utilisateur = await UtilisateurService.UtilisateurDeEmail(vue.Email);
            if (utilisateur == null)
            {
                // l'invité n'a pas de compte Kalosfide
                RetourDeService<Utilisateur> retourUser = await CréeUtilisateur(vue);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!retourUser.Ok)
                {
                    return SaveChangesActionResult(retourUser);
                }
                utilisateur = retourUser.Entité;
            }
            else
            {
                // l'utilisateur a un compte Kalosfide
                // on vérifie le mot de passe
                utilisateur = await UtilisateurService.UtilisateurVérifié(vue.Email, vue.Password);
                if (utilisateur == null)
                {
                    return RésultatBadRequest("Nom ou mot de passe invalide");
                }
            }

            // Vérifie que Nom et Adresse sont présents et non vides
            VérifieSansEspacesData(vue, Client.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // vérifie que le Nom est libre
            Client client = await _service.ClientDeNom(enregistrée.Id, vue.Nom);
            if (client != null && (enregistrée.ClientId == null || enregistrée.Client.Id != client.Id))
            {
                return RésultatBadRequest("nom", "nomPris");
            }

            // crée le Client
            RetourDeService retour = await _service.CréeClient(enregistrée.Id, utilisateur.Id, vue, enregistrée.Client);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            // Supprime l'invitation de la table
            await _service.SupprimeInvitation(enregistrée);

            // confirme l'email si ce n'est pas fait
            if (!utilisateur.EmailConfirmed)
            {
                await UtilisateurService.ConfirmeEmailDirect(utilisateur);
            }

            return await Connecte(utilisateur);
        }
    }

}
