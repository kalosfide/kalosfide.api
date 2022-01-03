using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
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
    public class CatégorieController : AvecIdUintController<Catégorie, CatégorieAAjouter, CatégorieAEditer>
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
        public new async Task<IActionResult> Ajoute(CatégorieAAjouter ajout)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(ajout.SiteId, EtatsRolePermis.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesData(ajout, Catégorie.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return await base.Ajoute( ajout);
        }

        [HttpPut("/api/categorie/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(CatégorieAEditer vue)
        {
            Catégorie catégorie = await _service.Lit(vue.Id);
            if (catégorie == null)
            {
                return NotFound();
            }
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(catégorie.SiteId, EtatsRolePermis.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesDataAnnulable(vue, Catégorie.AvérifierSansEspacesDataAnnulable);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return await Edite(catégorie, vue);
        }

        [HttpDelete("/api/categorie/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Supprime([FromQuery] uint id)
        {
            Catégorie catégorie = await _service.Lit(id);
            if (catégorie == null)
            {
                return NotFound();
            }
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(catégorie.SiteId, EtatsRolePermis.Actif);
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
        public async Task<IActionResult> NomPrisParAutre([FromQuery] uint id, string nom)
        {
            return Ok(await _service.NomPrisParAutre(id, nom));
        }
    }
}