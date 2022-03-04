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
        public async Task<IActionResult> Complet([FromQuery] uint id)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(id, PermissionsEtatRole.PasFermé);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Catalogue catalogue = await _service.Complet(id);

            return Ok(catalogue);
        }

        [HttpGet("/api/catalogue/disponible")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Disponible([FromQuery] uint id)
        {
            CarteUtilisateur carte = await CréeCarteUsager(id, PermissionsEtatRole.PasFermé, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Catalogue catalogue = await _service.Disponibles(id);

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
        public async Task<IActionResult> Etat([FromQuery] uint id)
        {
            CarteUtilisateur carte = await CréeCarteUsager(id, PermissionsEtatRole.PasFermé, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            return Ok(new ContexteCatalogue(carte.Site));
        }

        #endregion

        #region Modification

        /// <summary>
        /// Commence une modification du catalogue
        /// </summary>
        /// <param name="id">Id du site</param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/commence")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Commence([FromQuery] uint id)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(id, PermissionsEtatRole.Actif);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Site site = carteUtilisateur.Site;

            if (!site.Ouvert)
            {
                return RésultatBadRequest("Ouverture incorrecte");
            }

            RetourDeService<ArchiveSite> retour = await _siteService.CommenceEtatCatalogue(site);

            if (retour.Ok)
            {
                return RésultatCréé(new ContexteCatalogue(site));
            }
            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Termine la modification du catalogue
        /// </summary>
        /// <param name="id">Id du site</param>
        /// <returns></returns>
        [HttpPost("/api/catalogue/termine")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Termine([FromQuery] uint id)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(id, PermissionsEtatRole.Actif);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Site site = carteUtilisateur.Fournisseur.Site;

            if (site.Ouvert)
            {
                return RésultatBadRequest("Ouverture incorrecte");
            }

            // impossible de quitter l'état Catalogue si le site n'a pas de produits
            int produits = await _utile.NbDisponibles(site.Id);
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
                return RésultatCréé(new ContexteCatalogue(site));
            }
            return SaveChangesActionResult(retour);
        }

        #endregion
    }
}
