using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    [Route("api/[controller]")]
    [ApiValidationFilter]
    [Authorize]
    public class UtilisateurController : BaseController
    {
        private readonly IJwtFabrique _jwtFabrique;

        private readonly IUtilisateurService _service;

        private readonly ISiteService _siteService;

        private readonly IClientService _clientService;

        public UtilisateurController(
            IJwtFabrique jwtFabrique,
            IUtilisateurService service,
            ISiteService siteService,
            IClientService clientService
        )
        {
            _jwtFabrique = jwtFabrique;
            _service = service;
            _siteService = siteService;
            _clientService = clientService;
        }

        private async Task<RetourDeService<ApplicationUser>> CréeUser(ICréeCompteVue vue)
        {
            RetourDeService<ApplicationUser> retour = await _service.CréeUtilisateur(vue);

            if (retour.Type == TypeRetourDeService.IdentityError)
            {
                IEnumerable<IdentityError> errors = (IEnumerable<IdentityError>)retour.Objet;
                foreach (IdentityError error in errors)
                {
                    if (error.Code == IdentityErrorCodes.DuplicateUserName)
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, "nomPris", "email");
                    }
                    else
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, error.Code);
                    }
                }
            }
            return retour; 

        }

        private async Task<IActionResult> ResultAvecCarte(CarteUtilisateur carteUtilisateur)
        {
            JwtRéponse jwtRéponse = await _jwtFabrique.CréeReponse(carteUtilisateur);
            string header = JsonConvert.SerializeObject(jwtRéponse);
            Request.HttpContext.Response.Headers.Add(JwtFabrique.NomJwtRéponse, header);
            return new OkObjectResult(carteUtilisateur);
        }

        [HttpPost("ajoute")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Ajoute([FromBody]CréeCompteVue vue)
        {

            RetourDeService<ApplicationUser> retour = await CréeUser(vue);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }
            if (retour.Type != TypeRetourDeService.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            // envoie un mail contenant le lien de confirmation
            await _service.EnvoieEmailConfirmeCompte(retour.Entité);
            return Ok();
        }

        [HttpPost("confirmeEmail")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmeEmail([FromBody]ConfirmeEmailVue confirme)
        {
            ApplicationUser user = await _service.TrouveParId(confirme.Id);
            bool emailConfirmé = false;
            if (user != null)
            {
                emailConfirmé = await _service.EmailConfirmé(user, confirme.Code);
            }
            if (!emailConfirmé)
            {
                return RésultatBadRequest("Vous devez confirmer votre adresse email en cliquant sur le lien qui vous a été envoyé.");
            }
            return Ok();
        }

        [HttpPost("Connecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Connecte([FromBody]ConnectionVue connection)
        {
            ApplicationUser user = await _service.ApplicationUserVérifié(connection.Email, connection.Password);
            if (user == null)
            {
                return RésultatBadRequest("Nom ou mot de passe invalide");
            }
            if (!user.EmailConfirmed)
            { 
                return RésultatBadRequest("Vous devez confirmer votre adresse email en cliquant sur le lien qui vous a été envoyé.");
            }
            
            return await Connecte(user, connection.Persistant);
        }

        private async Task<IActionResult> Connecte(ApplicationUser user, bool persistant)
        {
            CarteUtilisateur carteUtilisateur = await _service.CréeCarteUtilisateur(user);

            if (!carteUtilisateur.EstUtilisateurActif)
            {
                var erreur = new 
                {
                    Champ = "etatUtilisateur",
                    Description = "Cet utilisateur n'est pas autorisé"
                };
                return StatusCode(403, erreur);
            }

            List<SiteVue> sites = carteUtilisateur.Sites;
            await _siteService.FixeNbs(sites);

            await _service.Connecte(user, persistant);

            return await ResultAvecCarte(carteUtilisateur);
        }

        [HttpPost("deconnecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        public async Task<IActionResult> Deconnecte()
        {
            await _service.Déconnecte();
            
            return Ok();
        }

        [HttpPost("créeSite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        public async Task<IActionResult> CréeSite([FromBody] CréeSiteVue vue)
        {
            if (!_service.VérifieTrimCréeSiteVue(vue))
            {
                return BadRequest();
            }

            CarteUtilisateur carteUtilisateur = await _service.CréeCarteUtilisateur(HttpContext.User);
            if (carteUtilisateur == null)
            {
                return Forbid();
            }

            if (!carteUtilisateur.EstUtilisateurActif)
            {
                var erreur = new
                {
                    Champ = "etatUtilisateur",
                    Description = "Cet utilisateur n'est pas autorisé"
                };
                return StatusCode(403, erreur);
            }

            Site site = await _siteService.TrouveParUrl(vue.Url);
            if (site != null)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "nomPris", "Url");
                return BadRequest(ModelState);
            }

            RetourDeService<Role> retour = await _service.CréeRoleSite(carteUtilisateur.Uid, vue);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            carteUtilisateur.AjouteRole(retour.Entité, retour.Entité.Site);
            return await ResultAvecCarte(carteUtilisateur);
        }

        private async Task<InvitationVérifiée> VérifieInviteClient(Invitation invitation)
        {
            InvitationVérifiée vérifiée = new InvitationVérifiée
            {
                Invitation = await _service.TrouveInvitation(invitation)
            };

            ApplicationUser user = await _service.TrouveParNom(invitation.Email);

            Site site = await _siteService.TrouveParKey(invitation.Uid, invitation.Rno);
            if (site == null)
            {
                vérifiée.Result = NotFound();
                return vérifiée;
            }
            vérifiée.Site = site;

            if (invitation.UidClient != null)
            {
                Client client = await _service.TrouveClientDansSite(site, invitation.UidClient, invitation.RnoClient.Value);
                if (client == null)
                {
                    vérifiée.Result = NotFound();
                    return vérifiée;
                }
                bool avecCompte = await _clientService.AvecCompte(client);
                if (avecCompte)
                {
                    ErreurDeModel.AjouteAModelState(ModelState, "Le client est déjà attribué à un utilisateur");
                    vérifiée.Result = BadRequest(ModelState);
                    return vérifiée;
                }
                vérifiée.Client = client;
            }

            if (user == null)
            {
                return vérifiée;
            }
            Utilisateur utilisateur = await _service.UtilisateurDeUser(user.Id);
            bool déjàClient = utilisateur.Roles
                .Where(r => r.Site.Uid == invitation.Uid && r.Site.Rno == invitation.Rno)
                .Any();
            if (déjàClient)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "L'utilisateur invité est déjà client du site");
                vérifiée.Result = BadRequest(ModelState);
                return vérifiée;
            }
            vérifiée.Utilisateur = utilisateur;
            return vérifiée;
        }

        [HttpPost("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(400)] // Bad request
        public async Task<IActionResult> Invitation([FromBody] Invitation invitation)
        {

            CarteUtilisateur carteUtilisateur = await _service.CréeCarteUtilisateur(HttpContext.User);
            if (carteUtilisateur == null)
            {
                return Forbid();
            }

            if (!carteUtilisateur.EstUtilisateurActif)
            {
                var erreur = new
                {
                    Champ = "etatUtilisateur",
                    Description = "Cet utilisateur n'est pas autorisé"
                };
                return StatusCode(403, erreur);
            }

            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Result != null)
            {
                return vérifiée.Result;
            }

            Invitation enregistrée = vérifiée.Invitation;

            await _service.EnvoieEmailDevenirClient(invitation, vérifiée);

            invitation.Date = DateTime.Now;

            RetourDeService<Invitation> retour = await _service.RemplaceInvitation(enregistrée, invitation);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return Ok(invitation);
        }

        [HttpGet("invitations")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Invitations([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _service.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (!await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                // pas le fournisseur
                return Forbid();
            }

            Site site = await _siteService.TrouveParKey(keySite.Uid, keySite.Rno);
            if (site == null)
            {
                return NotFound();
            }

            InvitationsStock invitations = new InvitationsStock
            {
                Invitations = await _service.Invitations(site),
                Date = DateTime.Now
            };

            return Ok(invitations);
        }

        [HttpDelete("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> AnnuleInvitation([FromQuery] InvitationKey key)
        {
            CarteUtilisateur carte = await _service.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            Site site = await _siteService.TrouveParKey(key.Uid, key.Rno);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                // pas le fournisseur
                return Forbid();
            }

            Invitation invitation = await _service.TrouveInvitation(key);
            if (invitation == null)
            {
                return NotFound();
            }

            RetourDeService retour = await _service.SupprimeInvitation(invitation);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }
            return Ok();
        }

        /// <summary>
        /// Retourne les informations sur le site et éventuellement du client si le code correspond à une invitation enregistrée
        /// pour pouvoir les afficher dans la page Devenir client
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("invitationClient")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> InvitationClient([FromQuery] string code)
        {

            Invitation invitation = _service.DécodeInvitation(code);
            if (invitation == null)
            {
                return Forbid();
            }

            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Result != null)
            {
                return vérifiée.Result;
            }

            // il doit y avoir une invitation enregistrée
            if (vérifiée.Invitation == null)
            {
                return NotFound();
            }

            InvitationClientVue invitationClient = new InvitationClientVue
            {
                Url = vérifiée.Site.Url,
                Titre = vérifiée.Site.Titre
            };
            if (vérifiée.Client != null)
            {
                invitationClient.Nom = vérifiée.Client.Nom;
                invitationClient.Adresse = vérifiée.Client.Adresse;
            }
            return Ok(invitationClient);
        }

        [HttpPost("devenirClient")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> DevenirClient([FromBody] DevenirClientVue vue)
        {
            if (!_service.VérifieTrimDevenirClientVue(vue))
            {
                return BadRequest();
            }
            if (vue.Code == null)
            {
                return Forbid();
            }
            Invitation invitation = _service.DécodeInvitation(vue.Code);
            if (invitation == null)
            {
                return Forbid();
            }

            if (vue.Email != invitation.Email)
            {
                return Forbid();
            }

            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Result != null)
            {
                return vérifiée.Result;
            }

            ApplicationUser user;
            if (vérifiée.Utilisateur == null)
            {
                RetourDeService<ApplicationUser> retourUser = await CréeUser(vue);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!retourUser.Ok)
                {
                    return SaveChangesActionResult(retourUser);
                }
                user = retourUser.Entité;
                vérifiée.Utilisateur = user.Utilisateur;
            }
            else
            {
                if (vérifiée.Invitation == null)
                {
                    return NotFound();
                }

                user = await _service.ApplicationUserVérifié(vue.Email, vue.Password);
                if (user == null)
                {
                    return RésultatBadRequest("Nom ou mot de passe invalide");
                }
            }


            if (await _service.VérifieNomPris(vérifiée.Site, vue.Nom, vérifiée.Client))
            {
                ErreurDeModel.AjouteAModelState(ModelState, "nomPris", "nom");
                return BadRequest(ModelState);
            }

            RetourDeService retour = await _service.CréeRoleClient(vérifiée.Site, vérifiée.Utilisateur, vérifiée.Client, vue);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            // confirme l'email si ce n'est pas fait
            if (!user.EmailConfirmed)
            {
                await _service.ConfirmeEmailDirect(user);
            }
            
            return await Connecte(user, true);
        }

        [HttpPost("oubliMotDePasse")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> OubliMotDePasse([FromBody] OubliMotDePasseVue vue)
        {
            ApplicationUser user = await _service.TrouveParNom(vue.Email);
            // n'envoie le mail que si la vue est valide
            if (user != null && user.EmailConfirmed && user.Email == vue.Email)
            {
                await _service.EnvoieEmailRéinitialiseMotDePasse(user);
            }
            // retourne toujours Ok pour ne pas envoyer d'information
            return Ok();
        }

        [HttpPost("reinitialiseMotDePasse")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> RéinitialiseMotDePasse([FromBody] RéinitialiseMotDePasseVue vue)
        {
            ApplicationUser user = await _service.TrouveParId(vue.Id);
            bool réinitialisé = false;
            if (user != null)
            {
                réinitialisé = await _service.RéinitialiseMotDePasse(user, vue.Code, vue.Password);
            }
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
            ApplicationUser user = await _service.TrouveParNom(vue.Email);

            if (await _service.ChangeMotDePasse(user, vue.Ancien, vue.Nouveau))
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
            ApplicationUser user = await _service.TrouveParNom(vue.Email);
            if (user != null)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "nomPris", "email");
                return BadRequest(ModelState);
            }
            user = await _service.TrouveParId(vue.Id);
            await _service.EnvoieEmailChangeEmail(user, vue.Email);
            return Ok();
        }

        [HttpPost("confirmeChangeEmail")]
        [ProducesResponseType(200)] // Ok
        public async Task<IActionResult> ConfirmeChangeEmail([FromBody] ConfirmeChangeEmailVue vue)
        {
            ApplicationUser user = await _service.TrouveParId(vue.Id);
            bool changé = false;
            if (user != null && vue.Email != null)
            {
                changé = await _service.ChangeEmail(user, vue.Email, vue.Code);
            }
            if (changé)
            {
                return Ok();
            }
            return StatusCode(500, "Changement impossible");
        }

        // GET api/utilisateur/?id
        [HttpGet("{id}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit(string id)
        {
            Utilisateur utilisateur = await _service.Lit(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            return Ok(utilisateur);
        }

        // GET api/utilisateur
        [HttpGet]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste()
        {
            return Ok(await _service.Lit());
        }

        // DELETE api/utilisateur/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)] // no content
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // 500 Internal Server Error
        public async Task<IActionResult> Supprime(string id)
        {
            CarteUtilisateur carte = await _service.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }
            if (!carte.EstAdministrateur)
            {
                return Forbid();
            }
            var utilisateur = await _service.Lit(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            var retour = await _service.Supprime(utilisateur);

            return SaveChangesActionResult(retour);
        }
    }
}
