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
    public class LivraisonController : CLFController
    {

        public LivraisonController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
            _type = "L";
            _typeBon = "C";
        }

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="keySite">key du site</param>
        /// <returns></returns>
        [HttpGet("/api/livraison/clients")]
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
        [HttpGet("/api/livraison/client")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public new async Task<IActionResult> Client([FromQuery] KeyUidRno keyClient)
        {
            return await base.Client(keyClient);
        }

        #endregion

        #region Action
        private async Task<IActionResult> CréeBonVirtuel(ParamsKeyClient paramsClient, bool copieLignes)
        {
            vérificateur.Initialise(paramsClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            DocCLF docACopier = null;
            // il ne peut y avoir qu'une seule livraison virtuelle pour le fournisseur
            KeyUidRnoNo key = new KeyUidRnoNo
            {
                Uid = vérificateur.KeyClient.Uid,
                Rno = vérificateur.KeyClient.Rno,
                No = 0
            };
            DocCLF livraison = await _service.DocCLFDeKey(key, "L");
            if (livraison != null)
            {
                return RésultatBadRequest("LivraisonVirtuellePrésente");
            }
            if (copieLignes)
            {
                docACopier = await _service.DernierDoc(vérificateur.KeyClient, "F");
                if (docACopier == null)
                {
                    return RésultatBadRequest("PasDeDernièreFacture");
                }
            }

            RetourDeService<CLFDoc> retour = await _service.AjouteBon(vérificateur.KeyClient, vérificateur.Site, "L", 0, docACopier);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        [HttpPut("/api/livraison/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Fixe([FromQuery] ParamsFixeLigne paramsLigne)
        {
            return await base.Fixe(paramsLigne);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour la ligne définie par la key.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/livraison/copie1")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Copie1([FromQuery] KeyUidRnoNo2 keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.CopieQuantité(ligneCLF, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieD([FromQuery] KeyUidRnoNo keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.CopieQuantité(docCLF, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieT(CLFDocsSynthèse clfDocs)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.CopieQuantité(docs, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Annule1([FromQuery] KeyUidRnoNo2 keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.Annule(ligneCLF, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleD([FromQuery] KeyUidRnoNo keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.Annule(docCLF, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleT(CLFDocsSynthèse clfDocs)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.Annule(docs, _type);
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
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Envoi(CLFDocsSynthèse clfDocs)
        {
            return await Synthèse(clfDocs);
        }

        #endregion

    }
}
