using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
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
    public class DocumentController : CLFController
    {

        public DocumentController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
        }

        /// <summary>
        /// Retourne la liste par client des bilans (nombre et total des montants) des documents par type.
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpGet("/api/document/bilans")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Bilans([FromQuery] KeyUidRno keySite)
        {
            // la liste est demandée par le fournisseur, paramsFiltre a la clé du site
            CarteUtilisateur carte = await CréeCarteFournisseur(keySite);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            List<CLFClientBilanDocs> bilans = await _service.ClientsAvecBilanDocuments(carte.Role.Site);

            return Ok(bilans);
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
                await UtilisateurEstClientPasFerméOuFournisseur();
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
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> ListeF([FromQuery] ParamsFiltreDoc paramsFiltre)
        {
            // la liste est demandée par le fournisseur, paramsFiltre a la clé du site
            CarteUtilisateur carte = await CréeCarteFournisseur(paramsFiltre);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            CLFDocs clfDocs = await _service.Résumés(paramsFiltre, carte.Role.Site);

            return Ok(clfDocs);
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés à l'utilisateur
        /// depuis sa dernière déconnection (bons de commande pour les sites dont l'utilisateur est fournisseur,
        /// bons de livraison et factures pour les sites dont l'utilisateur est client).
        /// La liste est dans l'ordre des dates.
        /// </summary>
        /// <param name="utilisateur">inclut les roles avec leurs site</param>
        /// <returns></returns>
        [HttpGet("/api/document/nouveaux")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> NouveauxDocs()
        {
            CarteUtilisateur carte = await CréeCarteUtilisateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            CLFDocs clfDocs = await _service.NouveauxDocs(carte.Utilisateur);

            return Ok(clfDocs);
        }

        private async Task<IActionResult> Document(KeyUidRnoNo keyDocSansType, string type)
        {
            if (keyDocSansType is null)
            {
                throw new System.ArgumentNullException(nameof(keyDocSansType));
            }

            vérificateur.Initialise(keyDocSansType);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientPasFerméOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFDocs document = await _service.Document(vérificateur.Site, keyDocSansType, type);
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
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Commande([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, TypeClf.Commande);
        }

        /// <summary>
        /// Retourne le bon de livraison
        /// </summary>
        /// <param name="keyDocument">key de la livraison</param>
        /// <returns></returns>
        [HttpGet("/api/document/livraison")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Livraison([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, TypeClf.Livraison);
        }

        /// <summary>
        /// Retourne la facture
        /// </summary>
        /// <param name="keyDocument">key de la facture</param>
        /// <returns></returns>
        [HttpGet("/api/document/facture")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Facture([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, TypeClf.Facture);
        }

        /// <summary>
        /// Cherche un document de type livraison ou facture à partir de la key de son site, de son Type et de son No.
        /// </summary>
        /// <param name="paramsChercheDoc">key du site, no et type du document</param>
        /// <returns>un CLFChercheDoc contenant la key et le nom du client et la date si le document recherché existe, vide sinon</returns>
        [HttpGet("/api/document/cherche")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Cherche([FromQuery] ParamsChercheDoc paramsChercheDoc)
        {
            // paramsChercheDoc a la clé du site
            CarteUtilisateur carte = await CréeCarteFournisseur(paramsChercheDoc);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            // seuls les type livraison et facture sont autorisés
            if (paramsChercheDoc.Type == TypeClf.Commande)
            {
                return BadRequest();
            }
            CLFChercheDoc chercheDoc = await _service.ChercheDocument(paramsChercheDoc);
            return Ok(chercheDoc);
        }

    }
}
