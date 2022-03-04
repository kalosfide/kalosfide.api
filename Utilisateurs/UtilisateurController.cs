using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    [Route("api/[controller]")]
    [ApiValidationFilter]
    [Authorize]
    public class UtilisateurController : AvecCarteController
    {
        private readonly ISiteService _siteService;

        private readonly IClientService _clientService;

        public UtilisateurController(
            IUtilisateurService utilisateurService,
            ISiteService siteService,
            IClientService clientService
        ) : base(utilisateurService)
        {
            _siteService = siteService;
            _clientService = clientService;
        }

        [HttpPost("connecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Connecte([FromBody]ConnectionVue connection)
        {
            Utilisateur utilisateur = await UtilisateurService.UtilisateurVérifié(connection.Email, connection.Password);
            if (utilisateur == null)
            {
                return RésultatBadRequest("Nom ou mot de passe invalide");
            }
            if (!utilisateur.EmailConfirmed)
            { 
                return RésultatBadRequest("Vous devez confirmer votre adresse email en cliquant sur le lien qui vous a été envoyé.");
            }
            
            return await Connecte(utilisateur);
        }

        [HttpPost("deconnecte")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> Deconnecte()
        {
            CarteUtilisateur carte = await CréeCarteUtilisateur();
            if (carte.Erreur == null)
            {
                // ne déconnecte que si l'utilisateur est connecté à sa session en cours
                await UtilisateurService.Déconnecte(carte);
            }

            // dans tous les cas
            return Ok();
        }

        [HttpGet("session")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> Session()
        {
            CarteUtilisateur carte = await CréeCarteUtilisateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok();
        }

        [HttpPost("idSite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> IdSite([FromQuery] uint id)
        {
            CarteUtilisateur carte;
            if (id == 0)
            {
                carte = await CréeCarteUtilisateur();
            }
            else
            {
                Site site = await _siteService.TrouveParId(id);
                if (site == null)
                {
                    return NotFound();
                }
                carte = await CréeCarteUsager(id, PermissionsEtatRole.PasFermé, PermissionsEtatRole.PasFermé);
            }
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            RetourDeService retour = await _utilisateurService.FixeIdDernierSite(carte.Utilisateur, id);
            return SaveChangesActionResult(retour);
        }

        #region Compte

        [HttpPost("ajoute")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Ajoute([FromBody]CréeCompteVue vue)
        {

            RetourDeService<Utilisateur> retour = await CréeUtilisateur(vue);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }
            if (retour.Type != TypeRetourDeService.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            // envoie un mail contenant le lien de confirmation
            await UtilisateurService.EnvoieEmailConfirmeCompte(retour.Entité);
            return Ok();
        }

        [HttpPost("confirmeEmail")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmeEmail([FromBody] ConfirmeEmailVue confirme)
        {
            Utilisateur user = await UtilisateurService.UtilisateurDeId(confirme.Id);
            bool emailConfirmé = false;
            if (user != null)
            {
                emailConfirmé = await UtilisateurService.EmailConfirmé(user, confirme.Code);
            }
            if (!emailConfirmé)
            {
                return RésultatBadRequest("Vous devez confirmer votre adresse email en cliquant sur le lien qui vous a été envoyé.");
            }
            return Ok();
        }

        [HttpPost("oubliMotDePasse")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> OubliMotDePasse([FromBody] OubliMotDePasseVue vue)
        {
            Utilisateur user = await UtilisateurService.UtilisateurDeEmail(vue.Email);
            // n'envoie le mail que si la vue est valide
            if (user != null && user.EmailConfirmed && user.Email == vue.Email)
            {
                await UtilisateurService.EnvoieEmailRéinitialiseMotDePasse(user);
            }
            // retourne toujours Ok pour ne pas envoyer d'information
            return Ok();
        }

        [HttpPost("réinitialiseMotDePasse")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> RéinitialiseMotDePasse([FromBody] RéinitialiseMotDePasseVue vue)
        {
            TokenDaté tokenDaté = _utilisateurService.DécodeTokenDaté(vue.Code);
            DateTime finValidité = tokenDaté.Date.Add(_utilisateurService.DuréeValiditéTokenRéinitialiseMotDePasse);
            if (finValidité < DateTime.Now)
            {
                return BadRequest("Code périmé");
            }

            bool réinitialisé = await UtilisateurService.RéinitialiseMotDePasse(vue.Id, tokenDaté.Token, vue.Password);
            if (!réinitialisé)
            {
                return BadRequest();
            }
            // retourne toujours Ok pour ne pas envoyer d'information
            return Ok();
        }

        [HttpPost("changeMotDePasse")]
        [ProducesResponseType(200)] // Ok
        public async Task<IActionResult> ChangeMotDePasse([FromBody] ChangeMotDePasseVue vue)
        {
            Utilisateur user = await UtilisateurService.UtilisateurDeEmail(vue.Email);

            if (await UtilisateurService.ChangeMotDePasse(user, vue.Ancien, vue.Nouveau))
            {
                return Ok();
            }
            return StatusCode(500, "Changement impossible");
        }

        [HttpPost("changeEmail")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailVue vue)
        {
            Utilisateur utilisateur = await UtilisateurService.UtilisateurDeEmail(vue.Email);
            if (utilisateur != null)
            {
                return RésultatBadRequest("email", "nomPris");
            }
            utilisateur = await UtilisateurService.UtilisateurDeId(vue.Id);
            await UtilisateurService.EnvoieEmailChangeEmail(utilisateur, vue.Email);
            return Ok();
        }

        [HttpPost("confirmeChangeEmail")]
        [ProducesResponseType(200)] // Ok
        public async Task<IActionResult> ConfirmeChangeEmail([FromBody] ConfirmeChangeEmailVue vue)
        {
            TokenDaté tokenDaté = _utilisateurService.DécodeTokenDaté(vue.Code);
            DateTime finValidité = tokenDaté.Date.Add(_utilisateurService.DuréeValiditéTokenRéinitialiseMotDePasse);
            if (finValidité < DateTime.Now)
            {
                return BadRequest("Code périmé");
            }
            bool changé = await UtilisateurService.ChangeEmail(vue.Id, vue.Email, tokenDaté.Token);
            if (changé)
            {
                return Ok();
            }
            return StatusCode(500, "Changement impossible");
        }

        #endregion // Compte

        // DELETE api/utilisateur/5
        [HttpDelete("{id}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Supprime(string id)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Utilisateur utilisateur = await UtilisateurService.UtilisateurDeId(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            var retour = await UtilisateurService.Supprime(utilisateur);
            return SaveChangesActionResult(retour);
        }
    }
}
