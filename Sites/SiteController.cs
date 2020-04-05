using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{

    [ApiController]
    [Route("UidRno")]
    [ApiValidationFilter]
    [Authorize]
    public class SiteController: KeyUidRnoController<Site, SiteVue>
    {
        private ISiteService _service { get => __service as ISiteService; }

        public SiteController(ISiteService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
        }

        private async Task<bool> EditeInterdit(CarteUtilisateur carte, KeyParam param)
        {
            return !carte.EstAdministrateur && !await carte.EstActifEtAMêmeUidRno(param);
        }

        protected override void FixePermissions()
        {
            dEditeInterdit = EditeInterdit;

        }

        [HttpGet("/api/site/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRno param)
        {
            return await base.Lit(param.KeyParam);
        }

        [HttpGet("/api/site/litNbs")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> LitNbs([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            bool estAdministrateur = carte.EstAdministrateur;
            bool estFournisseur = estAdministrateur ? false : await carte.EstActifEtAMêmeUidRno(keySite.KeyParam);
            if (!estAdministrateur && !estFournisseur)
            {
                return Forbid();
            }

            Site site = await _service.Lit(keySite.KeyParam);
            if (site == null)
            {
                return NotFound();
            }

            SiteVue vue = await _service.LitNbs(site);
            if (vue == null)
            {
                return NotFound();
            }

            return Ok(vue);
        }

        /// <summary>
        /// Reçoit une
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("/api/site/etat")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Etat([FromQuery] KeyUidRno keySite)
        {

            SiteVue vue = await _service.Etat(keySite);
            if (vue == null)
            {
                return NotFound();
            }

            return Ok(vue);
        }


        [HttpPut("/api/site/etat")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Etat(SiteVue vue)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            bool estAdministrateur = carte.EstAdministrateur;
            bool estFournisseur = estAdministrateur ? false : await carte.EstActifEtAMêmeUidRno(vue.KeyParam);
            if (!estAdministrateur && !estFournisseur)
            {
                return Forbid();
            }

            Site site = await _service.Lit(vue.KeyParam);
            if (site == null)
            {
                return NotFound();
            }

            await _service.ValideChangeEtat(site, vue, ModelState, estAdministrateur);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _service.ChangeEtat(site, vue.Etat));
        }

        [HttpGet("/api/site/liste")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public new async Task<IActionResult> Liste()
        {
            return await base.Liste();
        }

        [HttpPut("/api/site/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Edite(SiteVue vue)
        {
            return await base.Edite(vue);
        }

        [HttpGet("/api/site/trouveParNom/{nomSite}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> TrouveParNom(string nomSite)
        {
            SiteVue vue = await _service.TrouveParNom(nomSite);
            if (vue == null)
            {
                return NotFound();
            }
            return Ok(vue);
        }

        [HttpGet("/api/site/nomPris/{nomSite}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> NomPris(string nomSite)
        {
            return Ok(await _service.NomPris(nomSite));
        }

        [HttpGet("/api/site/nomPrisParAutre/{nomSite}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> NomPrisParAutre([FromQuery] KeyUidRno key, string nomSite)
        {
            return Ok(await _service.NomPrisParAutre(key, nomSite));
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
