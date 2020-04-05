using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KalosfideAPI.Catégories
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class CatégorieController : KeyUidRnoNoController<Catégorie, CatégorieVue>
    {

        public CatégorieController(ICatégorieService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
            FixePermissions();
        }

        private ICatégorieService _service { get => __service as ICatégorieService; }

        protected override void FixePermissions()
        {
            dEcritVerrouillé = EcritVerrouillé;
            dAjouteInterdit = InterditSiPasPropriétaire;
            dEditeInterdit = InterditSiPasPropriétaire;
            dSupprimeInterdit = InterditSiPasPropriétaire;
        }

        private async Task<bool> EcritVerrouillé(Catégorie donnée)
        {
            Site site = await _service.SiteDeDonnée(donnée);
            return site == null || site.Etat != TypeEtatSite.Catalogue;
        }

        [HttpPost("/api/categorie/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Ajoute(CatégorieVue vue)
        {
            return await base.Ajoute(vue);
        }

        [HttpGet("/api/categorie/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRnoNo param)
        {
            return await base.Lit(param.KeyParam);
        }

        [HttpPut("/api/categorie/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Edite(CatégorieVue vue)
        {
            return await base.Edite(vue);
        }

        [HttpDelete("/api/categorie/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Supprime([FromQuery] KeyParam paramsCatégorie)
        {
            IActionResult result = await base.Supprime(paramsCatégorie);
            return result;
        }

        [HttpGet("/api/categorie/nomPris/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPris(string nom)
        {
            return Ok(await _service.NomPris(nom));
        }

        [HttpGet("/api/categorie/nomPrisParAutre/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPrisParAutre([FromQuery] KeyUidRnoNo key, string nom)
        {
            return Ok(await _service.NomPrisParAutre(key, nom));
        }
    }
}