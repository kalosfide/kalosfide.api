using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private readonly IRoleService _roleService;

        public SiteController(ISiteService service, IRoleService roleService, IUtilisateurService utilisateurService) : base(utilisateurService)
        {
            _service = service;
            _roleService = roleService;
        }

        [HttpGet("/api/site/liste")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste()
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            List<SiteVue> liste = await _service.ListeVues();
            return Ok(liste);
        }

        /// <summary>
        /// Active ou désactive le Role du fournisseur du Site.
        /// </summary>
        /// <param name="key">KeyUidRno d'un Site et du Role du fournisseur de ce Site</param>
        /// <param name="activé">le Role est activé si true, désactivé sinon.</param>
        /// <returns></returns>
        [HttpPost("active")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Active([FromQuery] KeyUidRno key, bool activé)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            Role role = await _roleService.Lit(key);
            if (role == null)
            {
                return NotFound();
            }

            RetourDeService<RoleEtat> retour = await _roleService.ChangeEtat(role, activé ? TypeEtatRole.Actif : TypeEtatRole.Inactif);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return Ok(retour.Entité);
        }

        [HttpPost("ajoute")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> Ajoute([FromBody] CréeSiteVue vue)
        {
            CréeSiteVue.VérifieTrim(vue, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CarteUtilisateur carte = await CréeCarteUtilisateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            Site site = await _service.TrouveParUrl(vue.Url);
            if (site != null)
            {
                ErreurDeModel.AjouteAModelState(ModelState, "Url", "nomPris");
                return BadRequest(ModelState);
            }

            RetourDeService<Role> retour = await _service.CréeRoleSite(carte.Utilisateur, vue);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            carte.AjouteRole(retour.Entité);
            await _utilisateurService.AjouteCarteAResponse(Request.HttpContext.Response, carte);
            Identifiant identifiant = await carte.Identifiant();
            return new OkObjectResult(identifiant);
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
        public async Task<IActionResult> UrlPriseParAutre([FromQuery] KeyUidRno key, string Url)
        {
            return Ok(await _service.UrlPriseParAutre(key, Url));
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
        public async Task<IActionResult> TitrePrisParAutre([FromQuery] KeyUidRno key, string titre)
        {
            return Ok(await _service.TitrePrisParAutre(key, titre));
        }

    }
}
