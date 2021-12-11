using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    delegate void DVérifieEtatSite(Site site);
    delegate Task DActionPossible(Site site);
    delegate Task<RetourDeService> DAction(Site site);

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class CatalogueController : AvecCarteController
    {
        private readonly ICatalogueService _service;
        private readonly IUtileService _utile;
        private readonly ISiteService _siteService;

        public CatalogueController(ICatalogueService service,
            IUtileService utile,
            IUtilisateurService utilisateurService,
            ISiteService siteService
            ) : base(utilisateurService)
        {
            _service = service;
            _utile = utile;
            _siteService = siteService;
        }

        #region Lectures

        [HttpGet("/api/catalogue/complet")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Complet([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Catalogue catalogue = await _service.Complet(carteUtilisateur.Role.Site);

            return Ok(catalogue);
        }

        [HttpGet("/api/catalogue/disponible")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Disponible([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUsager(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Catalogue catalogue = await _service.Disponibles(carteUtilisateur.Role.Site);

            return Ok(catalogue);
        }

        /// <summary>
        /// Retourne un ContexteCatalogue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("/api/catalogue/etat")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Etat([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await CréeCarteUsager(keySite);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok(new ContexteCatalogue(carte.Role.Site));
        }

        #endregion

        #region Modification

        /// <summary>
        /// Commence une modification du catalogue
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/commence")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Commence([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Site site = carteUtilisateur.Role.Site;

            if (!site.Ouvert)
            {
                return RésultatBadRequest("EtatSiteIncorrect");
            }

            RetourDeService<ArchiveSite> retour = await _siteService.CommenceEtatCatalogue(site);

            if (retour.Ok)
            {
                return Ok(new ContexteCatalogue(site));
            }
            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Termine la modification du catalogue
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/termine")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Termine([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Site site = carteUtilisateur.Role.Site;
            if (site.Ouvert)
            {
                return RésultatBadRequest("EtatSiteIncorrect");
            }
            // impossible de quitter l'état Catalogue si le site n'a pas de produits
            int produits = await _utile.NbDisponibles(site);
            if (produits == 0)
            {
                return RésultatBadRequest("catalogueVide");
            }
            DateTime maintenant = DateTime.Now;
            bool modifié = await _service.ArchiveModifications(site, maintenant);
            DateTime? dateCatalogue = null;
            if (modifié)
            {
                dateCatalogue = maintenant;
            }
            RetourDeService<ArchiveSite> retour = await _siteService.TermineEtatCatalogue(site, dateCatalogue);

            if (retour.Ok)
            {
                return Ok(new ContexteCatalogue(site));
            }
            return SaveChangesActionResult(retour);
        }

        #endregion
    }
}
