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

        private async Task<RetourDeService<ApplicationUser>> CréeUser(ICréeCompteVue vue)
        {
            RetourDeService<ApplicationUser> retour = await UtilisateurService.CréeUtilisateur(vue);

            if (retour.Type == TypeRetourDeService.IdentityError)
            {
                IEnumerable<IdentityError> errors = (IEnumerable<IdentityError>)retour.Objet;
                foreach (IdentityError error in errors)
                {
                    if (error.Code == IdentityErrorCodes.DuplicateUserName)
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, "email", "nomPris");
                    }
                    else
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, error.Code);
                    }
                }
            }
            return retour; 

        }

        [HttpPost("connecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Connecte([FromBody]ConnectionVue connection)
        {
            ApplicationUser user = await UtilisateurService.ApplicationUserVérifié(connection.Email, connection.Password);
            if (user == null)
            {
                return RésultatBadRequest("Nom ou mot de passe invalide");
            }
            if (!user.EmailConfirmed)
            { 
                return RésultatBadRequest("Vous devez confirmer votre adresse email en cliquant sur le lien qui vous a été envoyé.");
            }
            
            return await Connecte(user);
        }

        private async Task<IActionResult> Connecte(ApplicationUser user)
        {
            CarteUtilisateur carte = await UtilisateurService.CréeCarteUtilisateur(user);

            if (!carte.EstUtilisateurActif)
            {
                var erreur = new 
                {
                    Champ = "etatUtilisateur",
                    Description = "Cet utilisateur n'est pas autorisé"
                };
                return StatusCode(403, erreur);
            }

            await UtilisateurService.Connecte(carte);

            return await ResultAvecCarte(carte);
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

        /// <summary>
        /// Vérifie que l'utilisateur invité n'est pas déjà client du site et s'il y a un compte existant, vérifie que ce compte n'est pas déjà attribué.
        /// </summary>
        /// <param name="invitation"></param>
        /// <returns>InvitationVérifiée avec le Site, l'Utilisateur défini par l'Email s'il existe et le Role à gérer s'il y en a un</returns>
        private async Task<InvitationVérifiée> VérifieInviteClient(Invitation invitation)
        {
            InvitationVérifiée vérifiée = new InvitationVérifiée
            {
                Invitation = await UtilisateurService.TrouveInvitation(invitation)
            };

            Site site = await _siteService.TrouveParKey(invitation.Uid, invitation.Rno);
            if (site == null)
            {
                vérifiée.Erreur = NotFound();
                return vérifiée;
            }
            vérifiée.Site = site;

            // s'il s'agit d'une invitation à gérer un compte existant, on vérifie si le compte existe et convient
            if (invitation.UidClient != null)
            {
                // on lit dans le bdd le Role avec Site et Utilisateur et éventuellement ApplicationUser
                Role client = await _clientService.LitRole(invitation.UidClient, invitation.RnoClient.Value);
                if (client == null)
                {
                    // le compte n'existe pas
                    vérifiée.Erreur = NotFound();
                    return vérifiée;
                }
                if (Role.EstFournisseur(client))
                {
                    // c'est le fournisseur
                    vérifiée.Erreur = RésultatBadRequest("Le role est celui d'un fournisseur");
                    return vérifiée;
                }
                if (!Role.EstUsager(client, site))
                {
                    vérifiée.Erreur = RésultatBadRequest("Le site du client n'est pas le site de l'invitation");
                    return vérifiée;
                }
                if (client.Utilisateur.ApplicationUser != null)
                {
                    vérifiée.Erreur = RésultatBadRequest("Le client est déjà attribué à un utilisateur");
                    return vérifiée;
                }
                vérifiée.Client = client;
            }

            // vérifie s'il existe un Utilisateur ayant l'Email de l'invitation 
            ApplicationUser user = await UtilisateurService.ApplicationUserDeEmail(invitation.Email);
            if (user == null)
            {
                // il n'y a pas d'ApplicationUser ayant le même Email que l'invitation
                return vérifiée;
            }
            // il y a un ApplicationUser ayant le même Email que l'invitation
            // on lit dans la bdd l'Utilisateur correspondant avec ses Roles
            Utilisateur utilisateur = await UtilisateurService.UtilisateurDeApplicationUser(user);
            bool déjàUsager= utilisateur.Roles
                .Where(r => r.Site.Uid == invitation.Uid && r.Site.Rno == invitation.Rno)
                .Any();
            if (déjàUsager)
            {
                // l'Utilisateur a déjà un role sur le Site
                vérifiée.Erreur = RésultatBadRequest("L'utilisateur invité est déjà usager du site");
                return vérifiée;
            }
            vérifiée.Utilisateur = utilisateur;
            return vérifiée;
        }

        [HttpPost("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> Invitation([FromBody] Invitation invitation)
        {
            // l'invitation a la même key que son site
            CarteUtilisateur carte = await CréeCarteFournisseur(invitation);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            // Vérifie que l'utilisateur invité n'est pas déjà client du site et s'il y a un compte existant, vérifie que ce compte n'est pas déjà attribué
            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Erreur != null)
            {
                return vérifiée.Erreur;
            }

            invitation.Date = DateTime.Now;

            RetourDeService<Invitation> retour = await UtilisateurService.EnvoieEmailDevenirClient(invitation, vérifiée);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return Ok(invitation);
        }

        [HttpGet("invitations")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Invitations([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(keySite);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            InvitationsStock invitations = new InvitationsStock
            {
                Invitations = await UtilisateurService.Invitations(carte.Role.Site),
                Date = DateTime.Now
            };

            return Ok(invitations);
        }

        [HttpDelete("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> AnnuleInvitation([FromQuery] InvitationKey key)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(key);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Invitation invitation = await UtilisateurService.TrouveInvitation(key);
            if (invitation == null)
            {
                return NotFound();
            }

            RetourDeService retour = await UtilisateurService.SupprimeInvitation(invitation);
            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Retourne les informations sur le site et éventuellement du client si le code correspond à une invitation enregistrée
        /// pour pouvoir les afficher dans la page Devenir client
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> InvitationClient([FromQuery] string code)
        {

            Invitation invitation = UtilisateurService.DécodeInvitation(code);
            if (invitation == null)
            {
                return RésultatBadRequest("Pas d'invitation");
            }

            // Vérifie que l'utilisateur invité n'est pas déjà client du site et s'il y a un compte existant, vérifie que ce compte n'est pas déjà attribué
            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Erreur != null)
            {
                return vérifiée.Erreur;
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
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> DevenirClient([FromBody] DevenirClientVue vue)
        {
            // Vérifie que Nom et Adresse sont présents et non vides
            Role.VérifieTrim(vue, ModelState);
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
            Invitation invitation = UtilisateurService.DécodeInvitation(vue.Code);
            if (invitation == null)
            {
                return absentOuInvalide;
            }
            if (vue.Email != invitation.Email)
            {
                return absentOuInvalide;
            }

            // Vérifie qu'il y a une invitation enregistrée qui est l'invitation de la vue
            Invitation enregistrée = await UtilisateurService.TrouveInvitation(invitation);
            if (enregistrée == null || enregistrée.Date != invitation.Date || enregistrée.UidClient != invitation.UidClient || enregistrée.RnoClient != invitation.RnoClient)
            {
                return absentOuInvalide;
            }

            // Vérifie que l'utilisateur invité n'est pas déjà client du site et s'il y a un compte existant, vérifie que ce compte n'est pas déjà attribué
            InvitationVérifiée vérifiée = await VérifieInviteClient(invitation);
            if (vérifiée.Erreur != null)
            {
                return vérifiée.Erreur;
            }

            ApplicationUser user;
            if (vérifiée.Utilisateur == null)
            {
                // l'invité n'a pas de compte Kalosfide
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
                // l'utilisateur a un compte Kalosfide
                // on vérifie le mot de passe
                user = await UtilisateurService.ApplicationUserVérifié(vue.Email, vue.Password);
                if (user == null)
                {
                    return RésultatBadRequest("Nom ou mot de passe invalide");
                }
            }

            // vérifie que le Nom est libre
            Role client = await _clientService.ClientDeNom(vérifiée.Site, vue.Nom);
            if (client != null && (vérifiée.Client == null || vérifiée.Client.Uid != client.Uid))
            {
                return RésultatBadRequest("nom", "nomPris");
            }

            // crée le Role
            RetourDeService retour = await _clientService.CréeRoleClient(vérifiée.Site, vérifiée.Utilisateur, vérifiée.Client, vue);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            // Supprime l'invitation de la table
            await UtilisateurService.SupprimeInvitation(enregistrée);

            // confirme l'email si ce n'est pas fait
            if (!user.EmailConfirmed)
            {
                await UtilisateurService.ConfirmeEmailDirect(user);
            }
            
            return await Connecte(user);
        }

        #region Compte

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
            await UtilisateurService.EnvoieEmailConfirmeCompte(retour.Entité);
            return Ok();
        }

        [HttpPost("confirmeEmail")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmeEmail([FromBody] ConfirmeEmailVue confirme)
        {
            ApplicationUser user = await UtilisateurService.ApplicationUserDeUserId(confirme.Id);
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
            ApplicationUser user = await UtilisateurService.ApplicationUserDeEmail(vue.Email);
            // n'envoie le mail que si la vue est valide
            if (user != null && user.EmailConfirmed && user.Email == vue.Email)
            {
                await UtilisateurService.EnvoieEmailRéinitialiseMotDePasse(user);
            }
            // retourne toujours Ok pour ne pas envoyer d'information
            return Ok();
        }

        [HttpPost("reinitialiseMotDePasse")]
        [ProducesResponseType(200)] // Ok
        [AllowAnonymous]
        public async Task<IActionResult> RéinitialiseMotDePasse([FromBody] RéinitialiseMotDePasseVue vue)
        {
            ApplicationUser user = await UtilisateurService.ApplicationUserDeUserId(vue.Id);
            bool réinitialisé = false;
            if (user != null)
            {
                réinitialisé = await UtilisateurService.RéinitialiseMotDePasse(user, vue.Code, vue.Password);
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
            ApplicationUser user = await UtilisateurService.ApplicationUserDeEmail(vue.Email);

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
            ApplicationUser user = await UtilisateurService.ApplicationUserDeEmail(vue.Email);
            if (user != null)
            {
                return RésultatBadRequest("email", "nomPris");
            }
            user = await UtilisateurService.ApplicationUserDeUserId(vue.Id);
            await UtilisateurService.EnvoieEmailChangeEmail(user, vue.Email);
            return Ok();
        }

        [HttpPost("confirmeChangeEmail")]
        [ProducesResponseType(200)] // Ok
        public async Task<IActionResult> ConfirmeChangeEmail([FromBody] ConfirmeChangeEmailVue vue)
        {
            ApplicationUser user = await UtilisateurService.ApplicationUserDeUserId(vue.Id);
            bool changé = false;
            if (user != null && vue.Email != null)
            {
                changé = await UtilisateurService.ChangeEmail(user, vue.Email, vue.Code);
            }
            if (changé)
            {
                return Ok();
            }
            return StatusCode(500, "Changement impossible");
        }

        #endregion // Compte

        // GET api/utilisateur/?id
        [HttpGet("{id}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit(string id)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Utilisateur utilisateur = await UtilisateurService.UtilisateurDeUid(id);
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
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok(await UtilisateurService.Lit());
        }

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

            var utilisateur = await UtilisateurService.UtilisateurDeUid(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            var retour = await UtilisateurService.Supprime(utilisateur);
            return SaveChangesActionResult(retour);
        }
    }
}
