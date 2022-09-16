using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Préférences
{
    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class PréférenceController: AvecCarteController
    {
        private IPréférenceService _service;
        public PréférenceController(IPréférenceService service, IUtilisateurService utilisateurService): base(utilisateurService)
        {
            _service = service;
        }

        [HttpPost("/api/preference/fixe")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        public async Task<IActionResult> Fixe(PréférenceData préférenceData)
        {
            CarteUtilisateur carteUtilisateur;
            string utilisateurId;
            if (préférenceData.PourTous == true)
            {
                carteUtilisateur = await CréeCarteFournisseur(préférenceData.SiteId, PermissionsEtatRole.PasInactif);
                utilisateurId = "tous";
            }
            else
            {
                carteUtilisateur = await CréeCarteUsager(préférenceData.SiteId, PermissionsEtatRole.PasInactif, PermissionsEtatRole.PasInactif);
                utilisateurId = carteUtilisateur.Utilisateur.Id;
            }
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur.Erreur;
            }

            Préférence préférence = await _service.Lit(utilisateurId, préférenceData.SiteId, préférenceData.Id);
            RetourDeService retour;
            if (préférence == null)
            {
                préférence = new Préférence
                {
                    UtilisateurId = utilisateurId
                };
                Préférence.CopieData(préférenceData, préférence);
                retour = await _service.Ajoute(préférence);
            }
            else
            {
                retour = await _service.FixeValeur(préférence, préférenceData.Valeur);
            }
            if (retour.Ok)
            {
                return Ok();
            }
            return SaveChangesActionResult(retour);
        }
    }
}
