using KalosfideAPI.Commandes;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Livraisons
{
    delegate void DVérifieEtatSite(Site site);
    delegate Task<Livraison> DLivraisonDeLAction(Site site);
    delegate Task<RetourDeService> DActionLivraison(Site site, Livraison livraison);

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class LivraisonController : BaseController
    {
        private readonly ILivraisonService _service;
        private readonly IUtileService _utile;
        private readonly IUtilisateurService _utilisateurService;
        private readonly ISiteService _siteService;

        public LivraisonController(ILivraisonService service,
            IUtileService utile,
            IUtilisateurService utilisateurService, 
            ISiteService siteService
            ) : base()
        {
            _service = service;
            _utile = utile;
            _utilisateurService = utilisateurService;
            _siteService = siteService;
        }

        #region Lecture

        /// <summary>
        /// retourne la vue de la dernière Livraison d'un Fournisseur
        /// </summary>
        /// <param name="keySite">key du fournisseur</param>
        /// <returns></returns>
        [HttpGet("/api/livraison/enCours")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> EnCours([FromQuery] KeyUidRno keySite)
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

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            return Ok(await _service.LivraisonVueEnCours(site));
        }

        /// <summary>
        /// retourne une vue contenant uniquement les contenant uniquement les dernières commandes ouvertes des clients avec compte
        /// </summary>
        /// <param name="keySite">key du Site</param>
        /// <returns></returns>
        [HttpGet("/api/livraison/avecCompte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> AvecCompte([FromQuery] KeyUidRno keySite)
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

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            return Ok(await _service.VueDesCommandesOuvertesDesClientsAvecCompte(site));
        }

        #endregion

        #region Action-Modèles

        /// <summary>
        /// modèle de vérification pour exécuter une action du fournisseur
        /// </summary>
        /// <param name="keyOuVueCommande"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private async Task<IActionResult> ActionFournisseur(AKeyUidRno keySite,
            DVérifieEtatSite vérifieEtatSite, DLivraisonDeLAction livraisonDeLAction, DActionLivraison actionLivraison)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
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

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            vérifieEtatSite(site);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Livraison livraison = await livraisonDeLAction(site);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RetourDeService retour = await actionLivraison(site, livraison);

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Action-Site

        private void VérifieEtatSiteCommence(Site site)
        {
            if (site.Etat != TypeEtatSite.Ouvert)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
            }
        }
        private void VérifieEtatSiteAnnule(Site site)
        {
            if (site.Etat != TypeEtatSite.Livraison)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
            }
        }
        private void VérifieEtatSiteTermine(Site site)
        {
            if (site.Etat != TypeEtatSite.Livraison)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
            }
        }

        private async Task<Livraison> LivraisonDeCommence(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison != null && !livraison.Date.HasValue)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "DernièreLivraisonPasTerminée");
            }
            return livraison;
        }
        private async Task<Livraison> LivraisonDeAnnule(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison == null || livraison.Date.HasValue)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "PasDeDernièreLivraisonOuTerminée");
                return null;
            }
            return livraison;
        }
        private async Task<Livraison> LivraisonDeTerminer(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison == null || livraison.Date.HasValue)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "PasDeDernièreLivraisonOuTerminée");
                return null;
            }
            bool estPrête = await _service.EstPrête(livraison);
            if (!estPrête)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "LivraisonPasPrête");
                return null;
            }
            return livraison;
        }

        private async Task<RetourDeService> ActionCommencer(Site site, Livraison livraison)
        {
            await _service.CommenceLivraison(site);
            return await _siteService.ChangeEtat(site, TypeEtatSite.Livraison);
        }
        private async Task<RetourDeService> ActionAnnule(Site site, Livraison livraison)
        {
            await _service.AnnuleLivraison(site, livraison);
            return await _siteService.ChangeEtat(site, TypeEtatSite.Ouvert);
        }
        private async Task<RetourDeService> ActionTermine(Site site, Livraison livraison)
        {
            await _service.TermineLivraison(site, livraison);
            return await _siteService.ChangeEtat(site, TypeEtatSite.Ouvert);
        }

        /// <summary>
        /// Commence une livraison
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/commence1")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Commence1([FromQuery] KeyUidRno keySite)
        {
            return await ActionFournisseur(keySite, VérifieEtatSiteCommence, LivraisonDeCommence, ActionCommencer);
        }

        /// <summary>
        /// Commence une livraison et retourne une vue contenant uniquement les dernières commandes ouvertes des clients avec compte
        /// </summary>
        /// <param name="keySite">key du Site</param>
        /// <returns></returns>
        [HttpPost("/api/livraison/commence")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Commence([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
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

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }
            if (site.Etat != TypeEtatSite.Ouvert)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
                return BadRequest(ModelState);
            }

            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison != null && !livraison.Date.HasValue)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "DernièreLivraisonPasTerminée");
            }

            await _service.CommenceLivraison(site);
            RetourDeService retour = await _siteService.ChangeEtat(site, TypeEtatSite.Livraison);

            if (retour.Ok)
            {
                return Ok(await _service.VueDesCommandesOuvertesDesClientsAvecCompte(site));
            }

            return SaveChangesActionResult(retour);

        }

        /// <summary>
        /// Annule la livraison commencée
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/annule")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Annule([FromQuery] KeyUidRno keySite)
        {
            return await ActionFournisseur(keySite, VérifieEtatSiteAnnule, LivraisonDeAnnule, ActionAnnule);
        }

        /// <summary>
        /// Termine la livraison commencée
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/termine")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Termine([FromQuery] KeyUidRno keySite)
        {
            return await ActionFournisseur(keySite, VérifieEtatSiteTermine, LivraisonDeTerminer, ActionTermine);
        }

        #endregion
    }
}
