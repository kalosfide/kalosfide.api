using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class FactureController : CLFController
    {

        public FactureController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
            _type = "F";
            _typeBon = "L";
        }

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="keySite">key du site</param>
        /// <returns></returns>
        [HttpGet("/api/facture/clients")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public new async Task<IActionResult> Clients([FromQuery] KeyUidRno keySite)
        {
            return await base.Clients(keySite);
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        [HttpGet("/api/facture/client")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public new async Task<IActionResult> Client([FromQuery] KeyUidRno keyClient)
        {
            return await base.Client(keyClient);
        }

        /// <summary>
        /// Crée une facture à partir des livraisons dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs.
        /// Fixe le NoGroupe de ces livraisons. La réponse contient un DocCLF contenant uniquement le No et la Date de la facture créée.
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <returns></returns>
        [HttpPost("/api/facture/envoi")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Envoi(CLFDocsSynthèse clfDocs)
        {
            return await Synthèse(clfDocs);
        }

        #endregion

    }
}
