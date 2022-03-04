using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class AdminController: AvecCarteController
    {
        private readonly IAdminService _service;

        public AdminController(
            IUtilisateurService utilisateurService,
            IAdminService adminService
        ) : base(utilisateurService)
        {
            _service = adminService;
        }

        /// <summary>
        /// Liste des fournisseurs.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/admin/fournisseurs")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Fournisseurs()
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteAdministrateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            List<FournisseurVue> fournisseurs = await _service.Fournisseurs();
            return Ok(fournisseurs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/admin/fournisseur")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Fournisseur([FromQuery] uint id)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteAdministrateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            FournisseurVue fournisseur = await _service.Fournisseur(id);
            if (fournisseur == null)
            {
                return NotFound();
            }
            return Ok(fournisseur);
        }
    }

}
