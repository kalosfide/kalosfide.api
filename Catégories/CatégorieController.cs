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
        }

        private ICatégorieService _service { get => __service as ICatégorieService; }

        [HttpPost("/api/categorie/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute(CatégorieVue vue)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(vue);
            return await Ajoute(carte, vue);
        }

        [HttpPut("/api/categorie/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(CatégorieVue vue)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(vue);
            Catégorie catégorie = await _service.Lit(vue);
            return await Edite(carte, catégorie, vue);
        }

        [HttpDelete("/api/categorie/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Supprime([FromQuery] KeyParam paramsCatégorie)
        {
            KeyUidRnoNo key = KeyParam.CréeKeyUidRnoNo(paramsCatégorie);
            if (key == null)
            {
                return BadRequest();
            }
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(key);
            Catégorie catégorie = await _service.Lit(key);
            return  await Supprime(carte, catégorie);
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