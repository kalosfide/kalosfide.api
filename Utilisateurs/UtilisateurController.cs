using KalosfideAPI.Data;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    [Route("api/[controller]")]
    [ApiValidationFilter]
    [Authorize]
    public class UtilisateurController : BaseController
    {
        protected readonly IJwtFabrique _jwtFabrique;

        protected readonly IUtilisateurService _service;

        protected readonly ISiteService _siteService;

        public UtilisateurController(
            IJwtFabrique jwtFabrique,
            IUtilisateurService service,
            ISiteService siteService
        )
        {
            _jwtFabrique = jwtFabrique;
            _service = service;
            _siteService = siteService;
        }

        [HttpPost("Connecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Connecte([FromBody]ConnectionVue connection)
        {
            ApplicationUser user = await _service.ApplicationUserVérifié(connection.UserName, connection.Password);
            if (user == null)
            {
                return RésultatBadRequest("Nom ou mot de passe invalide");
            }
            
            return await Connecte(user, connection.Persistant);
        }

        protected async Task<IActionResult> Connecte(ApplicationUser user, bool persistant)
        {
            CarteUtilisateur carteUtilisateur = await _service.CréeCarteUtilisateur(user);

            if (!carteUtilisateur.EstUtilisateurActif)
            {
                var erreur = new 
                {
                    Champ = "etatUtilisateur",
                    Description = "Cet utilisateur n'est pas autorisé"
                };
                return StatusCode(403, erreur);
            }

            List<SiteVue> sites = carteUtilisateur.Sites;
            await _siteService.FixeNbs(sites);

            JwtRéponse jwtRéponse = await _jwtFabrique.CréeReponse(carteUtilisateur);
            Request.HttpContext.Response.Headers.Add(JwtFabrique.NomJwtRéponse, JsonConvert.SerializeObject(jwtRéponse));
            await _service.Connecte(user, persistant);
            return new OkObjectResult(carteUtilisateur);
        }

        [HttpPost("deconnecte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        public async Task<IActionResult> Deconnecte()
        {
            await _service.Déconnecte();
            
            return Ok();
        }

        // GET api/utilisateur/?id
        [HttpGet("{id}")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit(string id)
        {
            Utilisateur utilisateur = await _service.Lit(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            return Ok(utilisateur);
        }

        // GET api/utilisateur
        [HttpGet]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Liste()
        {
            return Ok(await _service.Lit());
        }

        // DELETE api/utilisateur/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)] // no content
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // 500 Internal Server Error
        public async Task<IActionResult> Supprime(string id)
        {
            var utilisateur = await _service.Lit(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            var retour = await _service.Supprime(utilisateur);

            return SaveChangesActionResult(retour);
        }
    }
}
