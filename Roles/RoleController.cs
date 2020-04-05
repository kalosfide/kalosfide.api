using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Roles
{

    [Route("api/[controller]/[action]/{param?}")]
    [ApiController]
    [ApiValidationFilter]
    [Authorize]
    public class RoleController : KeyUidRnoController<Role, RoleVue>
    {
        private IRoleService _service { get => __service as IRoleService; }

        public RoleController(IRoleService service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
            FixePermissions();
        }

        protected override void FixePermissions()
        {
            dAjouteInterdit = Interdiction;
            dEditeInterdit = Interdiction;
            dSupprimeInterdit = Interdiction;
            dListeInterdit = Interdiction;
            dLitInterdit = Interdiction;
        }

        [HttpPost]
        [ProducesResponseType(200)] // ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // forbidden
        [ProducesResponseType(404)] // not found
        public async Task<IActionResult> ChangeEtat([FromQuery] KeyUidRno param, [FromBody] string etat)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }
            if (!TypeEtatRole.EstValide(etat))
            {
                return BadRequest();
            }

            var donnée = await _service.Lit(param.KeyParam);
            if (donnée == null)
            {
                return NotFound();
            }

            bool permis = (donnée.EstFournisseur && carte.EstAdministrateur) || (donnée.EstClient && await carte.EstActifEtAMêmeUidRno(donnée.SiteParam));
            if (!permis)
            {
                return Forbid();
            }

            var retour = await _service.ChangeEtat(donnée, etat);

            return SaveChangesActionResult(retour);
        }
    }
}