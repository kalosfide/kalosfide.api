using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class DocumentController : CLFController
    {

        public DocumentController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du client qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date et a la même key que le client</param>
        /// <returns></returns>
        [HttpGet("/api/document/client")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> ListeC([FromQuery] ParamsFiltreDoc paramsFiltre)
        {
            // la liste est demandée par le client, paramsFiltre a la clé du client
            vérificateur.Initialise(paramsFiltre);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClient();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFDocs clfDocs = await _service.Résumés(paramsFiltre, vérificateur.Client);

            return Ok(clfDocs);
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du site qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date et a la même key que le site</param>
        /// <returns></returns>
        [HttpGet("/api/document/clients")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> ListeF([FromQuery] ParamsFiltreDoc paramsFiltre)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            // la liste est demandée par le fournisseur, paramsFiltre a la clé du site
            if (!await carte.EstActifEtAMêmeUidRno(paramsFiltre.KeyParam))
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKey(paramsFiltre);
            if (site == null)
            {
                return NotFound();
            }

            CLFDocs clfDocs = await _service.Résumés(paramsFiltre, site);

            return Ok(clfDocs);
        }

        private async Task<IActionResult> Document(KeyUidRnoNo keyDocument, string type)
        {
            vérificateur.Initialise(keyDocument);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFDocs document = await _service.Document(vérificateur.Site, keyDocument, type);
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);

        }

        /// <summary>
        /// Retourne le bon de commande
        /// </summary>
        /// <param name="keyDocument">key de la commande</param>
        /// <returns></returns>
        [HttpGet("/api/document/commande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Commande([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, "C");
        }

        /// <summary>
        /// Retourne le bon de livraison
        /// </summary>
        /// <param name="keyDocument">key de la livraison</param>
        /// <returns></returns>
        [HttpGet("/api/document/livraison")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Livraison([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, "L");
        }

        /// <summary>
        /// Retourne la facture
        /// </summary>
        /// <param name="keyDocument">key de la facture</param>
        /// <returns></returns>
        [HttpGet("/api/document/facture")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Facture([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, "F");
        }

    }
}
