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
    public class RoleController : AvecCarteController
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service, IUtilisateurService utilisateurService) : base(utilisateurService)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(200)] // ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> ChangeEtat([FromQuery] KeyUidRno keyRole, [FromBody] string etat)
        {
            CarteUtilisateur carte = await CréeCarteUtilisateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            if (!TypeEtatRole.EstValide(etat))
            {
                return BadRequest();
            }

            Role role = await _service.Lit(keyRole);
            if (role == null)
            {
                return NotFound();
            }

            string message = Role.EstFournisseur(role) && !carte.EstAdministrateur
                ? "Seul un administrateur peut changer l'état d'un fournisseur."
                : Role.EstClient(role) && !(await carte.EstFournisseurActif(role.Site))
                    ? "Seul le fournisseur du site peut changer l'état d'un client d'un site."
                    : null;
            if (message != null)
            {
                return RésultatInterdit(message);
            }

            RetourDeService<RoleEtat> retour = await _service.ChangeEtat(role, etat);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }
            return Ok(retour.Entité);
        }
    }
}