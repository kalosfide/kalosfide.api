using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KalosfideAPI.Produits
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class ProduitController : KeyUidRnoNoController<Produit, ProduitVue>
    {
        public ProduitController(IProduitService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
        }

        private IProduitService Service { get => __service as IProduitService; }

        [HttpPost("/api/produit/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute(ProduitVue vue)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(vue);
            return await Ajoute(carte, vue);
        }

        [HttpPut("/api/produit/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(ProduitVue vue)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(vue);
            Produit produit = await Service.Lit(vue);
            IActionResult result = await Edite(carte, produit, vue);
            if (result is OkObjectResult)
            {
                if (vue.Etat != null && vue.Etat != TypeEtatProduit.Disponible)
                // supprime les détails des dernières commandes des clients qui demandent le produit
                {
                    await Service.SupprimeDétailsCommandesSansLivraison(vue);
                }
            }
            return result;
        }

        [HttpDelete("/api/produit/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Supprime([FromQuery] KeyParam paramsProduit)
        {
            KeyUidRnoNo key = KeyParam.CréeKeyUidRnoNo(paramsProduit);
            if (key == null)
            {
                return BadRequest();
            }
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(key);
            Produit produit = await Service.Lit(key);
            return await Supprime(carte, produit);
        }

        [HttpGet("/api/produit/nomPris/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPris(ProduitVue vue)
        {
            return Ok(await Service.NomPris(vue.Uid, vue.Rno, vue.Nom));
        }

        [HttpGet("/api/produit/nomPrisParAutre/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPrisParAutre(ProduitVue vue)
        {
            return Ok(await Service.NomPrisParAutre(vue.Uid, vue.Rno, vue.No, vue.Nom));
        }
    }
}