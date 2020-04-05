using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
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
            FixePermissions();
        }

        private IProduitService _service { get => __service as IProduitService; }

        protected override void FixePermissions()
        {
            dEcritVerrouillé = EcritVerrouillé;
            dAjouteInterdit = InterditSiPasPropriétaire;
            dEditeInterdit = InterditSiPasPropriétaire;
            dSupprimeInterdit = InterditSiPasPropriétaire;
        }

        private async Task<bool> EcritVerrouillé(Produit donnée)
        {
            Site site = await _service.SiteDeDonnée(donnée);
            return site == null || site.Etat != TypeEtatSite.Catalogue;
        }


        [HttpPost("/api/produit/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Ajoute(ProduitVue vue)
        {
            return await base.Ajoute(vue);
        }

        [HttpGet("/api/produit/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRnoNo param)
        {
            return await base.Lit(param.KeyParam);
        }

        [HttpPut("/api/produit/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Edite(ProduitVue vue)
        {
            IActionResult result = await base.Edite(vue);
            if (result is OkObjectResult)
            {
                if (vue.Etat != null && vue.Etat != Data.Constantes.TypeEtatProduit.Disponible)
                // supprime les détails des dernières commandes des clients qui demandent le produit
                {
                    await _service.SupprimeDétailsCommandesSansLivraison(vue);
                }
            }
            return result;
        }

        [HttpDelete("/api/produit/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Supprime([FromQuery] KeyParam paramsProduit)
        {
            IActionResult result = await base.Supprime(paramsProduit);
            return result;
        }

        [HttpGet("/api/produit/nomPris/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPris(ProduitVue vue)
        {
            return Ok(await _service.NomPris(vue.Uid, vue.Rno, vue.Nom));
        }

        [HttpGet("/api/produit/nomPrisParAutre/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPrisParAutre(ProduitVue vue)
        {
            return Ok(await _service.NomPrisParAutre(vue.Uid, vue.Rno, vue.No, vue.Nom));
        }
    }
}