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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/document/bilans")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Bilans([FromQuery] uint id)
        {
            // la liste est demandée par le fournisseur
            CarteUtilisateur carte = await CréeCarteFournisseur(id, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            List<CLFClientBilanDocs> bilans = await _service.ClientsAvecBilanDocuments(carte.Fournisseur.Site);

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
            vérificateur.Initialise(paramsFiltre.Id);
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
            CarteUtilisateur carte = await CréeCarteFournisseur(paramsFiltre.Id, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            CLFDocs clfDocs = await _service.Résumés(paramsFiltre, carte.Fournisseur.Site);

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

        /// <summary>
        /// Retourne le bon de commande
        /// </summary>
        /// <param name="keyDocument">key du document</param>
        /// <returns></returns>
        [HttpGet("/api/document/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyDoc keyDocument)
        {
            if (keyDocument is null)
            {
                throw new System.ArgumentNullException(nameof(keyDocument));
            }

            vérificateur.Initialise(keyDocument);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientPasFerméOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFPdfAEnvoyer àEnvoyer = await _service.CLFPdfAEnvoyer(keyDocument, vérificateur.EstClient);
            if (àEnvoyer == null)
            {
                return NotFound();
            }

            return Ok(àEnvoyer);
        }

        /// <summary>
        /// Cherche un document de type livraison ou facture à partir de la key de son site, de son Type et de son No.
        /// </summary>
        /// <param name="paramsChercheDoc">Contient l'Id du site, no et type du document</param>
        /// <returns>un CLFChercheDoc contenant l'Id et le nom du client et la date si le document recherché existe, vide sinon</returns>
        [HttpGet("/api/document/cherche")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Cherche([FromQuery] ParamsChercheDoc paramsChercheDoc)
        {
            // paramsChercheDoc a la clé du site
            CarteUtilisateur carte = await CréeCarteFournisseur(paramsChercheDoc.Id, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            // seuls les type livraison et facture sont autorisés
            if (paramsChercheDoc.Type == TypeCLF.Commande)
            {
                return BadRequest();
            }
            CLFDoc chercheDoc = await _service.ChercheDocument(paramsChercheDoc);
            return Ok(chercheDoc);
        }


        [HttpPost("/api/document/téléchargé")]
        [ProducesResponseType(201)] // Created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Téléchargé(KeyDoc keyDocument)
        {
            vérificateur.Initialise(keyDocument);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientPasFerméOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            Partages.RetourDeService retour = await _service.Téléchargement(keyDocument, vérificateur.EstClient);
            if (retour == null)
            {
                return NotFound();
            }
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }
            return Ok(keyDocument);

        }


    }
}
