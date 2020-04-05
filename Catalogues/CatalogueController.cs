using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    delegate void DVérifieEtatSite(Site site);
    delegate Task DActionPossible(Site site);
    delegate Task<RetourDeService> DAction(Site site);

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class CatalogueController : BaseController
    {
        private readonly ICatalogueService _service;
        private readonly IUtileService _utile;
        private readonly IUtilisateurService _utilisateurService;
        private readonly ISiteService _siteService;

        public CatalogueController(ICatalogueService service,
            IUtileService utile,
            IUtilisateurService utilisateurService,
            ISiteService siteService
            ) : base()
        {
            _service = service;
            _utilisateurService = utilisateurService;
            _utile = utile;
            _siteService = siteService;
        }

        #region Lectures

        [HttpGet("/api/catalogue/complet")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Complet([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            bool estFournisseur = await carte.EstActifEtAMêmeUidRno(keySite.KeyParam);
            if (!estFournisseur)
            {
                return Forbid();
            }

            Catalogue catalogue = await _service.Complet(keySite);
            if (catalogue == null)
            {
                return NotFound();
            }

            return Ok(catalogue);
        }

        [HttpGet("/api/catalogue/disponible")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Disponible([FromQuery] KeyUidRno keySite)
        {
            Catalogue catalogue = await _service.Disponibles(keySite);
            if (catalogue == null)
            {
                return NotFound();
            }

            return Ok(catalogue);
        }

        [HttpGet("/api/catalogue/obsolete")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Obsolète([FromQuery] KeyParam param)
        {

            Site site = await _siteService.Lit(param);
            if (site == null)
            {
                return NotFound();
            }

            return Ok(await _service.Obsolète(site, param.Date));
        }

        #endregion

        #region Modification

        /// <summary>
        /// modèle de vérification pour exécuter une action du fournisseur
        /// </summary>
        /// <param name="keyOuVueCommande"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private async Task<IActionResult> ActionFournisseur(AKeyUidRno keySite,
            DVérifieEtatSite vérifieEtatSite, DActionPossible actionPossible, DAction action)
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

            Site site = await _siteService.Lit(keySite.KeyParam);
            if (site == null)
            {
                return NotFound();
            }

            vérifieEtatSite(site);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await actionPossible(site);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RetourDeService retour = await action(site);

            return SaveChangesActionResult(retour);
        }

        private void VérifieEtatSiteCommence(Site site)
        {
            if (site.Etat != TypeEtatSite.Ouvert)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
            }
        }
        private void VérifieEtatSiteTermine(Site site)
        {
            if (site.Etat != TypeEtatSite.Catalogue)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "EtatSiteIncorrect");
            }
        }
        private Task CommencerPossible(Site site)
        {
            return Task.CompletedTask;
        }
        private async Task TerminerPossible(Site site)
        {
            // impossible de quitter l'état Catalogue si le site n'a pas de produits
            int produits = await _utile.NbDisponibles(site);
            if (produits == 0)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "catalogueVide");
            }
        }

        private async Task<RetourDeService> Commencer(Site site)
        {
            return await _siteService.ChangeEtat(site, TypeEtatSite.Catalogue);
        }
        private async Task<RetourDeService> Terminer(Site site)
        {
            await _service.ArchiveModifications(site);
            return await _siteService.ChangeEtat(site, TypeEtatSite.Ouvert);
        }

        /// <summary>
        /// Commence une modification du catalogue
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/commence")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Commence([FromQuery] KeyUidRno keySite)
        {
            return await ActionFournisseur(keySite, VérifieEtatSiteCommence, CommencerPossible, Commencer);
        }

        /// <summary>
        /// Termine la modification du catalogue
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/termine")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Termine([FromQuery] KeyUidRno keySite)
        {
            return await ActionFournisseur(keySite, VérifieEtatSiteTermine, TerminerPossible, Terminer);
        }

        #endregion
    }
}
