using KalosfideAPI.Erreurs;
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
    public class SiteController: AvecCarteController
    {
        private readonly ISiteService _service;

        public SiteController(ISiteService service, IUtilisateurService utilisateurService) : base(utilisateurService)
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

    }
}
