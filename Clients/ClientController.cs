using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
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
    public class ClientController : KeyUidRnoController<Client, ClientVue>
    {
        private readonly IUtileService _utile;

        public ClientController(IClientService service,
            IUtileService utile,
            IUtilisateurService utilisateurService
            ) : base(service, utilisateurService)
        {
            _utile = utile;
            FixePermissions();
        }

        private IClientService _service { get => __service as IClientService; }

        protected override void FixePermissions()
        {
            dSupprimeInterdit = Interdiction;
            dListeInterdit = Interdiction;
            dLitInterdit = Interdiction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        [HttpGet("/api/client/ajoute")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute([FromQuery] ClientVueAjoute vue)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }
            // seul le fournisseur du site peut créer un client
            if (!await carte.EstActifEtAMêmeUidRno(vue.KeyParam))
            {
                return Forbid();
            }

            await _service.ValideAjoute(vue, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur();
            if (!retourUtilisateur.Ok)
            {
                return SaveChangesActionResult(retourUtilisateur);
            }

            RetourDeService<Client> retour = await _service.Ajoute(retourUtilisateur.Entité, vue);
            if (!retour.Ok)
            {
                await _utilisateurService.Supprime(retourUtilisateur.Entité);
                return SaveChangesActionResult(retour);
            }

            return Ok(retour.Entité);
        }

        [HttpPut("/api/client/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Edite(ClientVue vue)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyParam paramSite = await _service.KeyParamDuSiteDuClient(vue.KeyParam);
            // sont autorisés: le client et le fournisseur du site s'il a créé le client
            bool estClient = await carte.EstActifEtAMêmeUidRno(vue.KeyParam);
            if (!estClient)
            {
                bool estFournisseur = await carte.EstActifEtAMêmeUidRno(paramSite);
                if (!estFournisseur)
                {
                    // l'utilisateur n'est ni le client ni le fournisseur du site
                    return Forbid();
                }
                if (await _service.AvecCompte(vue))
                {
                    // l'utilisateur est le fournisseur du site mais n'a pas créé le client
                    return Forbid();
                }
            }

            Client donnée = await _service.Lit(vue.KeyParam);
            if (donnée == null)
            {
                return NotFound();
            }

            Client àValider = _service.CréeDonnéeEditéeComplète(vue, donnée);

            await _service.ValideEdite(paramSite.CréeKeyUidRno(), àValider, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RetourDeService<Client> retour = await _service.Edite(donnée, vue);

            return SaveChangesActionResult(retour);
        }

        [HttpPut("/api/client/etat")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Etat(ClientEtatVue vue)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyParam paramSite = await _service.KeyParamDuSiteDuClient(vue.KeyParam);
            bool estFournisseur = await carte.EstActifEtAMêmeUidRno(paramSite);
            if (!estFournisseur)
            {
                // seul le fournisseur peut changer l'état d'un client
                return Forbid();
            }

            Role role = await _service.Role(vue.KeyParam);
            if (role == null)
            {
                return NotFound();
            }

            if (vue.Etat == TypeEtatRole.Actif)
            {
                if (role.Etat != TypeEtatRole.Nouveau && role.Etat != TypeEtatRole.Inactif)
                {
                    return BadRequest();
                }
            }
            else
            {
                if (vue.Etat != TypeEtatRole.Exclu)
                {
                    return BadRequest();
                }
                if (role.Etat != TypeEtatRole.Nouveau && role.Etat != TypeEtatRole.Actif)
                {
                    return BadRequest();
                }
            }

            RetourDeService<Role> retour = await _service.ChangeEtat(role, vue.Etat);

            return SaveChangesActionResult(retour);
        }

        [HttpGet("/api/client/liste")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (! await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                return Forbid();
            }

            List<ClientEtatVue> clients = await _service.ClientsDuSite(keySite);
            return Ok(new
            {
                Clients = clients,
                Date = DateTime.Now
            });
        }

        [HttpGet("/api/client/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRno keyClient)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (carte.Uid != keyClient.Uid || !carte.Roles.Exists(r => r.Rno == keyClient.Rno))
            {
                return Forbid();
            }

            ClientVue vue = await _service.LitVue(keyClient);
            return Ok(vue);
        }

        [HttpGet("/api/client/depuis")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Depuis([FromQuery] KeyUidRnoDate keySiteDate)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyUidRno keySite = new KeyUidRno
            {
                Uid = keySiteDate.Uid,
                Rno = keySiteDate.Rno
            };

            if (!await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            List<ClientEtatVue> clients = await _service.NouveauxClients(keySite, keySiteDate.Date);
            return Ok(new
            {
                Clients = clients,
                Date = DateTime.Now
            });
        }

    }
}
