using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class AdminController: AvecCarteController
    {
        private readonly IAdminService _service;
        private readonly IRoleService _roleService;

        public AdminController(
            IUtilisateurService utilisateurService,
            IAdminService adminService,
            IRoleService roleService
        ) : base(utilisateurService)
        {
            _service = adminService;
            _roleService = roleService;
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

            List<Fournisseur> fournisseurs = await _service.Fournisseurs();
            return Ok(fournisseurs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyRole"></param>
        /// <returns></returns>
        [HttpGet("/api/admin/fournisseur")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Fournisseur([FromQuery] KeyUidRno keyRole)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteAdministrateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Fournisseur fournisseur = await _service.Fournisseur(keyRole);
            if (fournisseur == null)
            {
                return NotFound();
            }
            return Ok(fournisseur);
        }

        [HttpPost("/api/admin/active")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Active(KeyUidRnoActif keyRoleActif)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteAdministrateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Role role = await _roleService.Lit(keyRoleActif);
            if (role == null)
            {
                return NotFound();
            }

            if (!Role.EstFournisseur(role))
            {
                return BadRequest();
            }

            string état;
            if (keyRoleActif.Actif)
            {
                if (role.Etat == TypeEtatRole.Actif)
                {
                        return BadRequest();
                }
                état = TypeEtatRole.Actif;
            }
            else
            {
                if (role.Etat != TypeEtatRole.Actif)
                {
                        return BadRequest();
                }
                état = TypeEtatRole.Inactif;
            }

            RetourDeService<RoleEtat> retour = await _roleService.ChangeEtat(role, état);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return Ok(retour.Entité);
        }
    }

}
