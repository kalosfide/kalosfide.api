using KalosfideAPI.Data;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{

    [ApiController]
    [Route("UidRno")]
    [ApiValidationFilter]
    [Authorize]
    public class SiteController: AvecIdUintController<Site, ISiteData, Site, SiteAEditer>
    {
        private readonly ISiteService _service;

        public SiteController(ISiteService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
            _service = service;
        }

        [HttpGet("/api/site/urlPrise/{Url}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> UrlPrise(string Url)
        {
            return Ok(await _service.UrlPrise(Url));
        }

        [HttpGet("/api/site/urlPriseParAutre/{Url}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> UrlPriseParAutre([FromQuery] uint id, string Url)
        {
            return Ok(await _service.UrlPriseParAutre(id, Url));
        }

        [HttpGet("/api/site/titrePris/{titre}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> TitrePris(string titre)
        {
            return Ok(await _service.TitrePris(titre));
        }

        [HttpGet("/api/site/titrePrisParAutre/{titre}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> TitrePrisParAutre([FromQuery] uint id, string titre)
        {
            return Ok(await _service.TitrePrisParAutre(id, titre));
        }

        [HttpGet("/api/site/avecInvitations")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> AvecInvitations([FromQuery] uint id)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteFournisseur(id, PermissionsEtatRole.PasFermé);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }
            return Ok(await _service.AvecInvitations(id));
        }

        [HttpPut("/api/site/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(SiteAEditer vue)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(vue.Id, PermissionsEtatRole.PasInactif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            VérifieSansEspacesDataAnnulable(vue, Site.AvérifierSansEspacesDataAnnulable);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Site donnée = await _service.Lit(vue.Id);
            if (donnée == null)
            {
                return NotFound();
            }

            if (donnée.Url != null && donnée.Url != vue.Url && await _service.AvecInvitations(vue.Id) > 0)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "url", "modifImpossible");
                return BadRequest(ModelState);
            }
            return await Edite(donnée, vue);
        }


    }
}
