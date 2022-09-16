using System.Threading.Tasks;
using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Préférences;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KalosfideAPI.Produits
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class ProduitController : AvecIdUintController<Produit, ProduitAAjouter, ProduitAEnvoyer, ProduitAEditer>
    {
        private readonly IPréférenceService _préférenceService;
        private readonly ICatégorieService _catégorieService;
        public ProduitController(ICatégorieService catégorieService, IPréférenceService préférenceService, IProduitService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
            _catégorieService = catégorieService;
            _préférenceService = préférenceService;
        }

        private IProduitService Service { get => __service as IProduitService; }

        [HttpPost("/api/produit/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async new Task<IActionResult> Ajoute(ProduitAAjouter ajout)
        {
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(ajout.SiteId, PermissionsEtatRole.PasInactif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesData(ajout, Produit.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool avecCatégories = await _préférenceService.AvecCatégories(ajout.SiteId);
            if (!avecCatégories)
            {
                string nomSansCatégorie = _préférenceService.NomSansCatégorie();
                Catégorie catégorieSansCatégorie = await _catégorieService.CatégorieDeNom(nomSansCatégorie);
                uint idSansCatégorie;
                if (catégorieSansCatégorie == null)
                {
                    CatégorieAAjouter catégorieAAjouter = new CatégorieAAjouter
                    {
                        Nom = nomSansCatégorie,
                        SiteId = ajout.SiteId
                    };
                    RetourDeService<CatégorieAEnvoyer> retour = await _catégorieService.Ajoute(catégorieAAjouter, ModelState);
                    if (!retour.Ok)
                    {
                        return SaveChangesActionResult(retour);
                    }
                    idSansCatégorie = retour.Entité.Id;
                }
                else
                {
                    idSansCatégorie = catégorieSansCatégorie.Id;
                }
                ajout.CategorieId = idSansCatégorie;
            }
            return await base.Ajoute(ajout);
        }

        [HttpPut("/api/produit/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(ProduitAEditer édité)
        {
            Produit produit = await Service.Lit(édité.Id);
            if (produit == null)
            {
                return NotFound();
            }
            bool disponible = produit.Disponible;
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(produit.SiteId, PermissionsEtatRole.PasInactif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesDataAnnulable(édité, Produit.AvérifierSansEspacesDataAnnulable);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IActionResult result = await Edite(produit, édité);
            if (result is OkObjectResult)
            {
                if (!produit.Disponible && disponible)
                // supprime les lignes des dernières commandes des clients qui demandent le produit devenu indisponible
                {
                    await Service.SupprimeLignesCommandesPasEnvoyées(produit);
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
        public async Task<IActionResult> Supprime([FromQuery] uint id)
        {
            Produit produit = await Service.Lit(id);
            if (produit == null)
            {
                return NotFound();
            }
            CarteUtilisateur carte = await CréeCarteFournisseurCatalogue(produit.SiteId, PermissionsEtatRole.PasInactif);
            return await Supprime(carte, produit);
        }

        [HttpGet("/api/produit/nomPris/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPris(ProduitAAjouter vue)
        {
            return Ok(await Service.NomPris(vue.SiteId, vue.Nom));
        }

        [HttpGet("/api/produit/nomPrisParAutre/{nom}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NomPrisParAutre(ProduitAEditer vue)
        {
            Produit produit = await Service.Lit(vue.Id);
            if (produit == null)
            {
                return NotFound();
            }
            return Ok(await Service.NomPrisParAutre(produit.SiteId, produit.Id, vue.Nom));
        }
    }
}