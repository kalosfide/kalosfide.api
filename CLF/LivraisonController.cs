using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
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
    public class LivraisonController : CLFController
    {

        public LivraisonController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
            _type = TypeCLF.Livraison;
        }

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="idSite">key du site</param>
        /// <returns></returns>
        [HttpGet("/api/livraison/clients")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public new async Task<IActionResult> Clients([FromQuery] uint idSite)
        {
            return await base.Clients(idSite);
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="idClient"></param>
        /// <returns></returns>
        [HttpGet("/api/livraison/client")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public new async Task<IActionResult> Client([FromQuery] uint idClient)
        {
            return await base.Client(idClient);
        }

        #endregion

        #region Action

        /// <summary>
        /// Crée une nouvelle livraison virtuelle vide pour le client défini par la clé
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/livraison/nouveau")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Nouveau([FromQuery] ParamsKeyClient paramsClient)
        {
            return await CréeBon(paramsClient, false);
        }

        /// <summary>
        /// Crée une nouvelle livraison virtuelle pour le client défini par la clé avec des détails copiés sur ceux de la facture précédente
        /// dont les produits sont toujours disponibles si cette facture a été créée par un bon de livraison virtuel seul
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/livraison/clone")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Clone([FromQuery] ParamsKeyClient paramsClient)
        {
            return await CréeBon(paramsClient, true);
        }

        /// <summary>
        /// Efface toutes les lignes du bon et si le bon est virtuel, supprime le bon.
        /// </summary>
        /// <param name="paramsBon"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/efface")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Efface([FromQuery] ParamsKeyDoc paramsBon)
        {
            return await EffaceBon(paramsBon);
        }

        [HttpPut("/api/livraison/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite(CLFLigne clfLigne)
        {
            vérificateur.Initialise(clfLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
                await LigneExiste();
                DocEstASynthétiser();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.EditeLigne(vérificateur.LigneCLF, clfLigne);

            return SaveChangesActionResult(retour);
        }

        [HttpPost("/api/livraison/fixe")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Fixe([FromQuery] ParamsFixeLigne paramsLigne)
        {
            return await base.Fixe(paramsLigne);
        }

        [HttpDelete("/api/livraison/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Supprime([FromQuery] ParamsSupprimeLigne paramsSupprime)
        {
            return await base.Supprime(paramsSupprime);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour la ligne définie par la key.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/copie1")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Copie1([FromQuery] KeyLigneSansType keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.CopieQuantité(ligneCLF, TypeCLF.Livraison);
            return await Action(keyLigne, action);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour chaque ligne du document défini par la key.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/copieD")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieD([FromQuery] KeyDocSansType keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.CopieQuantité(docCLF, TypeCLF.Livraison);
            return await Action(keyDoc, action);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour chaque ligne des documents 
        /// dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/copieT")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieT(ParamsSynthèse clfDocs)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.CopieQuantité(docs, TypeCLF.Livraison);
            return await Action(clfDocs, action);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/annule1")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Annule1([FromQuery] KeyLigneSansType keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.Annule(ligneCLF, TypeCLF.Livraison);
            return await Action(keyLigne, action);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/annuleD")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleD([FromQuery] KeyDocSansType keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.Annule(docCLF, TypeCLF.Livraison);
            return await Action(keyDoc, action);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents 
        /// dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/annuleT")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleT(ParamsSynthèse clfDocs)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.Annule(docs, TypeCLF.Livraison);
            return await Action(clfDocs, action);
        }

        /// <summary>
        /// Crée une livraison à partir des commandes dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs.
        /// Fixe le NoGroupe de ces commandes. La réponse contient un DocCLF contenant uniquement le No et la Date de la livraison créée.
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/envoi")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Envoi(ParamsSynthèse clfDocs)
        {
            return await Synthèse(clfDocs);
        }

        #endregion

    }
}
