using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.NouveauxSites
{
    public class NouveauSiteController : AvecCarteController
    {
        private readonly INouveauSiteService nouveauSiteService;
        private readonly IEnvoieEmailService emailService;
        private readonly ISiteService siteService;

        public NouveauSiteController(
            INouveauSiteService nouveauSiteService,
            IEnvoieEmailService emailService,
            IUtilisateurService utilisateurService,
            ISiteService siteService
            ) : base(utilisateurService)
        {
            this.nouveauSiteService = nouveauSiteService;
            this.emailService = emailService;
            this.siteService = siteService;
        }

        /// <summary>
        /// Enregistre une demande de création d'un nouveau site.
        /// </summary>
        /// <param name="vue">NouveauSiteVue définissant le site à créer</param>
        /// <returns></returns>
        [HttpPost("demande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Demande(NouveauSiteDemande vue)
        {
            NouveauSite nouveauSite = await nouveauSiteService.NouveauSite(vue.Email);
            if (nouveauSite != null)
            {
                return RésultatBadRequest("Il y a déjà une demande de création de site pour cet email.");
            }

            Site.VérifieTrim(vue, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await siteService.UrlPrise(vue.Url))
            {
                return RésultatBadRequest("Il y a déjà un site avec cette url.");
            }
            if (await nouveauSiteService.UrlPrise(vue.Url))
            {
                return RésultatBadRequest("Il y a déjà une demande de création de site avec cette url.");
            }
            if (await siteService.TitrePris(vue.Titre))
            {
                return RésultatBadRequest("Il y a déjà un site avec ce titre.");
            }
            if (await nouveauSiteService.TitrePris(vue.Titre))
            {
                return RésultatBadRequest("Il y a déjà une demande de création de site avec ce titre.");
            }

            Role.VérifieTrim(vue, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RetourDeService retour = await nouveauSiteService.EnregistreDemande(vue);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return Ok();
        }

        [HttpPost("invite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Invite([FromQuery] string email)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            NouveauSite nouveauSite = await nouveauSiteService.NouveauSite(email);
            if (nouveauSite == null)
            {
                return NotFound();
            }

            string objet = "Création du site: " + nouveauSite.Titre;
            string urlBase = ClientApp.Url(ClientApp.DevenirClient);
            string message = "Vous pouvez finaliser la création de votre site: " + nouveauSite.Titre;

            await emailService.EnvoieEmail<NouveauSite>(email, objet, message, urlBase, nouveauSite, null);

            return Ok();
        }

        [HttpGet("invitation")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Invitation([FromQuery] string code)
        {
            NouveauSiteDemande demande = emailService.DécodeCodeDeEmail<NouveauSiteDemande>(code);
            if (demande == null)
            {
                return RésultatBadRequest("Pas de site.");
            }

            NouveauSite enregistré = await nouveauSiteService.NouveauSite(demande.Email);
            if (enregistré == null)
            {
                return NotFound();
            }
            Role.CopieDef(enregistré, demande);
            Site.CopieDef(enregistré, demande);
            return Ok(demande);
        }

        [HttpPost("active")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Active(NouveauSiteActive nouveauSiteActive)
        {
            // Vérifie que l'invitation est présente et valide
            IActionResult absentOuInvalide = RésultatBadRequest("Code d'activation absent ou invalide");
            if (nouveauSiteActive.Code == null)
            {
                return absentOuInvalide;
            }
            NouveauSite nouveauSiteDécodé = emailService.DécodeCodeDeEmail<NouveauSite>(nouveauSiteActive.Code);
            if (nouveauSiteDécodé.Email != nouveauSiteActive.Email)
            {
                return absentOuInvalide;
            }
            NouveauSite nouveauSite = await nouveauSiteService.NouveauSite(nouveauSiteActive.Email);
            if (nouveauSite == null)
            {
                return NotFound();
            }
            if (nouveauSiteDécodé.Date != nouveauSite.Date)
            {
                return absentOuInvalide;
            }


            // vérifie s'il existe un Utilisateur ayant l'Email de l'invitation 
            ApplicationUser user = await UtilisateurService.ApplicationUserDeEmail(nouveauSiteActive.Email);
            Utilisateur utilisateur = null;
            if (user != null)
            {
                // il y a un ApplicationUser ayant le même Email que l'invitation
                // on vérifie le mot de passe
                user = await UtilisateurService.ApplicationUserVérifié(nouveauSiteActive.Email, nouveauSiteActive.Password);
                if (user == null)
                {
                    return RésultatBadRequest("Mot de passe invalide");
                }
                // on lit dans la bdd l'Utilisateur correspondant avec ses Roles
                utilisateur = await UtilisateurService.UtilisateurDeApplicationUser(user);
            }
            if (user == null)
            {
                // il n'y a pas d'ApplicationUser ayant le même Email que l'invitation
                // on ajoute à la bdd un nouvel Utilisateur et son ApplicationUser
                RetourDeService<ApplicationUser> retourUser = await UtilisateurService.CréeUtilisateur(nouveauSiteActive);
                if (!retourUser.Ok)
                {
                    return SaveChangesActionResult(retourUser);
                }
                user = retourUser.Entité;
                utilisateur = user.Utilisateur;
                utilisateur.Roles = new List<Role>();
            }

            RetourDeService<Role> retourRole = await nouveauSiteService.CréeRoleEtSite(utilisateur, nouveauSite);
            if (!retourRole.Ok)
            {
                return SaveChangesActionResult(retourRole);
            }

            // confirme l'email si ce n'est pas fait
            if (!user.EmailConfirmed)
            {
                await UtilisateurService.ConfirmeEmailDirect(user);
            }

            CarteUtilisateur carte = await UtilisateurService.CréeCarteUtilisateur(user);
            await UtilisateurService.Connecte(carte);

            return await ResultAvecCarte(carte);
        }
    }
}
