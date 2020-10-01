using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class CommandeController : CLFController
    {
        public CommandeController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(service, utile, utilisateurService)
        {
            _type = "C";
        }

        #region Client-Lecture

        /// <summary>
        /// Si le site est d'état Catalogue, retourne un contexte Catalogue: état site = Catalogue, date catalogue = DateNulle.
        /// Si le site est ouvert et si l'utilisateur a passé la date de son catalogue
        /// et si la date du catalogue utilisateur est postérieure à celle du catalogue de la bdd, les données utilisateur sont à jour,
        /// retourne un contexte Ok: état site = ouvert, date catalogue = DataNulle.
        /// Si le site est ouvert et si l'utilisateur a passé la date de son catalogue
        /// et si la date du catalogue utilisateur est antérieure à celle du catalogue de la bdd
        /// retourne un contexte Périmé: état site = ouvert, date catalogue = DataNulle.
        /// Si le site est ouvert et si l'utilisateur n'a pas passé la date de son catalogue, il n'y pas de données utilisateur,
        /// retourne un CLFDocs dont le champ Documents contient les données pour client de la dernière commande du client
        /// </summary>
        /// <param name="paramsKeyClient">key du client et date de son catalogue</param>
        /// <returns></returns>
        [HttpGet("/api/commande/enCours")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> EnCours([FromQuery] ParamsKeyClient paramsKeyClient)
        {
            vérificateur.Initialise(paramsKeyClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClient();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFDocs docs = await _service.CommandeEnCours(vérificateur.Site, paramsKeyClient, paramsKeyClient.DateCatalogue);

            return Ok(docs);
        }

        #endregion

        #region Action-Vérificateurs

        /// <summary>
        /// si l'utilisateur est un client, vérifie qu'il n'y a pas eu de livraison ou de modification de l'état du site ou du catalogue
        /// depuis que l'utilisateur a chargé les données.
        /// Si changé, fixe la date du catalogue du vérificateur à la valeur de celle en cours pour que l'action puisse retourner le contexte.
        /// </summary>
        private async Task<CLFDocs> EtatSiteChangé()
        {
            // cas d'un client
            if (vérificateur.EstClient)
            {
                DateTime date = await _utile.DateCatalogue(vérificateur.Site);
                // vérifie que le site est ouvert et que la catalogue n'est pas plus récent que la commande
                if (vérificateur.Site.Etat != TypeEtatSite.Ouvert || vérificateur.DateCatalogue < date)
                {
                    vérificateur.DateCatalogue = date;
                    return new CLFDocs
                    {
                        
                        Site = new Site { Etat = vérificateur.Site.Etat },
                        Catalogue = new Catalogues.Catalogue { Date = date }
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Vérifie que la date de vérificateur.Doc est absente si l'utilisateur est le client et DateNulle.Date si l'utilisateur est le fournisseur
        /// </summary>
        private void CommandeModifiable()
        {
            DocCLF doc = vérificateur.DocCLF;
            if (vérificateur.EstClient)
            {
                if (doc.Date.HasValue || vérificateur.KeyDoc.No == 0)
                {
                    vérificateur.Erreur = RésultatBadRequest("CommandeEnvoyéeOuVirtuelle");
                    throw new VérificationException();
                }
            }
            else
            {
                switch (doc.Type)
                {
                    case "C":
                        if (doc.No != 0)
                        {
                            vérificateur.Erreur = RésultatBadRequest("CommandeNonVirtuelle");
                            throw new VérificationException();
                        }
                        break;
                    case "L":
                        if (doc.Date.HasValue)
                        {
                            vérificateur.Erreur = RésultatBadRequest("LivraisonEnvoyée");
                            throw new VérificationException();
                        }
                        break;
                    case "F":
                        vérificateur.Erreur = RésultatBadRequest("ModifieFacture");
                        throw new VérificationException();
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Vérifie que vérificateur.DocCLF.Lignes ne contient pas déjà la ligne
        /// </summary>
        private void LigneNExistePas()
        {
            List<LigneCLF> lignes = new List<LigneCLF>(vérificateur.DocCLF.Lignes);
            LigneCLF ligne = lignes.Find(l => l.No2 == vérificateur.KeyLigne.No2);
            if (ligne != null)
            {
                vérificateur.Erreur = RésultatBadRequest("LigneExiste");
                throw new VérificationException();
            }
        }

        /// <summary>
        /// vérifie que le produit demandé par une ligne existe et est disponible
        /// </summary>
        private async Task PeutCommanderProduit()
        {
            Produit produit = await _utile.Produit(vérificateur.Site, vérificateur.KeyLigne.No2);
            if (produit == null || produit.Etat != TypeEtatProduit.Disponible)
            {
                vérificateur.Erreur = RésultatBadRequest("Produit");
                throw new VérificationException();
            }
            vérificateur.ArchiveProduit = produit.ArchiveProduits.OrderBy(a => a.Date).Last();
        }

        /// <summary>
        /// lit le détail s'il existe
        /// </summary>
        private async Task LitLIgne()
        {
            vérificateur.LigneCLF = await _service.LigneCLFDeKey(vérificateur.KeyLigne, _type);
        }

        /// <summary>
        /// Vérifie que les champs requis sont présents dans la ligne postée.
        /// </summary>
        private void ChampsRequisPrésents()
        {
            bool ajout = vérificateur.LigneCLF == null;
            if (ajout)
            {
                if (!vérificateur.CLFLigne.Quantité.HasValue)
                {
                    if (vérificateur.EstClient)
                    {
                        vérificateur.Erreur = RésultatBadRequest("QuantitéRequis");
                        throw new VérificationException();
                    }
                    else
                    {
                        // le fournisseur peut envoyer ALivrer seul défini
                        if (!vérificateur.CLFLigne.AFixer.HasValue)
                        {
                            vérificateur.Erreur = RésultatBadRequest("QuantitéOuAFixerRequis");
                            throw new VérificationException();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// vérifie que les champs qui vont être modifiés peuvent l'être
        /// </summary>
        private void ChampsInterditsAbsents()
        {
            if (vérificateur.EstClient)
            {
                if (vérificateur.CLFLigne.AFixer.HasValue)
                {
                    vérificateur.Erreur = RésultatBadRequest("EstClientEtAFixerPrésent");
                    throw new VérificationException();
                }
            }
        }

        /// <summary>
        /// vérifie que les valeurs des champs présents sont valides
        /// </summary>
        private void ChampsPrésentsValides()
        {
            if (vérificateur.CLFLigne.TypeCommande != null && !TypeUnitéDeCommande.DemandeEstValide(vérificateur.CLFLigne.TypeCommande, vérificateur.ArchiveProduit.TypeCommande))
            {
                vérificateur.Erreur = RésultatBadRequest("invalide", "typeCommande");
                throw new VérificationException();
            }

            string code;
            if (vérificateur.CLFLigne.Quantité.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.CLFLigne.Quantité.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest(code, "quantité");
                    throw new VérificationException();
                }
            }

            if (vérificateur.CLFLigne.AFixer.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.CLFLigne.AFixer.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest(code, "aFixer");
                    throw new VérificationException();
                }
            }

        }

        #endregion

        #region Action-Commande
        private async Task<IActionResult> CréeCommande(ParamsKeyClient paramsClient, bool copieLignes)
        {
            vérificateur.Initialise(paramsClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            long noCommande;
            DocCLF docACopier = null;
            if (vérificateur.EstClient)
            {
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                } 
                DocCLF dernièreCommande = await _service.DernierDoc(vérificateur.KeyClient, "C");
                if (copieLignes)
                {
                    // la dernière commande doit exister et être envoyée
                    if (dernièreCommande == null || !dernièreCommande.Date.HasValue)
                    {
                        return RésultatBadRequest("DerniereCommandeAbsenteOuPasEnvoyée");
                    }
                    docACopier = dernièreCommande;
                }
                else
                {
                    // la dernière commande doit ne pas exister ou être envoyée.
                    if (!(dernièreCommande == null || dernièreCommande.Date.HasValue))
                    {
                        return RésultatBadRequest("DerniereCommandePrésenteEtPasEnvoyée");
                    }
                }
                noCommande = dernièreCommande == null ? 1 : dernièreCommande.No + 1;
            }
            else
            {
                // il ne peut y avoir qu'une seule commande virtuelle pour le fournisseur
                KeyUidRnoNo key = new KeyUidRnoNo
                {
                    Uid = vérificateur.KeyClient.Uid,
                    Rno = vérificateur.KeyClient.Rno,
                    No = 0
                };
                DocCLF commande = await _service.DocCLFDeKey(key, "C");
                if (commande != null)
                {
                    return RésultatBadRequest("CommandeVirtuellePrésente");
                }
                if (copieLignes)
                {
                    docACopier = await _service.DernierDoc(vérificateur.KeyClient, "L");
                    if (docACopier == null)
                    {
                        return RésultatBadRequest("PasDeDernièreLivraison");
                    }
                }
                noCommande = 0;
            }

            RetourDeService<CLFDoc> retour = await _service.AjouteBon(vérificateur.KeyClient, vérificateur.Site, "C", noCommande, docACopier);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Fixe la date du bon de commande du client défini par la clé
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/envoi")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Envoi([FromQuery] ParamsKeyClient paramsClient)
        {
            vérificateur.Initialise(paramsClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }
            if (vérificateur.EstClient)
            {
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                }
                DocCLF dernièreCommande = await _service.DernierDoc(vérificateur.KeyClient, "C");
                // la dernière commande doit exister et ne pas être envoyée
                if (dernièreCommande == null || dernièreCommande.Date.HasValue)
                {
                    return RésultatBadRequest("DerniereCommandeAbsenteOuEnvoyée");
                }

                RetourDeService<CLFDoc> retour = await _service.EnvoiCommande(dernièreCommande);

                if (retour.Ok)
                {
                    return Ok(retour.Entité);
                }

                return SaveChangesActionResult(retour);
            }
            else
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Crée une nouvelle commande vide pour le client défini par la clé
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/nouveau")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Nouveau([FromQuery] ParamsKeyClient paramsClient)
        {
            return await CréeCommande(paramsClient, false);
        }

        /// <summary>
        /// Crée une nouvelle commande pour le client défini par la clé avec des détails copiés sur ceux de la commande
        /// précédente dont les produits sont toujours disponibles
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/clone")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Clone([FromQuery] ParamsKeyClient paramsClient)
        {
            return await CréeCommande(paramsClient, true);
        }


        /// <summary>
        /// Supprime la commande.
        /// </summary>
        /// <param name="paramsCommande"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/efface")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Efface([FromQuery] ParamsKeyDoc paramsCommande)
        {
            vérificateur.Initialise(paramsCommande);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
                await DocExiste();
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                }
                CommandeModifiable();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.SupprimeCommande(vérificateur.DocCLF);

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Action-Ligne

        [HttpPost("/api/commande/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute([FromQuery] ParamsVide paramsLigne, CLFLigne ligne)
        {
            vérificateur.Initialise(ligne, paramsLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                }
                await DocExiste();
                LigneNExistePas();
                CommandeModifiable();
                await PeutCommanderProduit();
                ChampsInterditsAbsents();
                ChampsRequisPrésents();
                ChampsPrésentsValides();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            ligne.Date = vérificateur.ArchiveProduit.Date;

            RetourDeService<CLFLigne> retour = await _service.AjouteLigne(ligne);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        [HttpPut("/api/commande/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite([FromQuery] ParamsVide paramsLigne, CLFLigne ligne)
        {
            vérificateur.Initialise(ligne, paramsLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                }
                await LigneExiste();
                CommandeModifiable();
                await PeutCommanderProduit();
                ChampsInterditsAbsents();
                ChampsPrésentsValides();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService<LigneCLF> retour = await _service.EditeLigne(vérificateur.LigneCLF, ligne);

            return SaveChangesActionResult(retour);
        }

        [HttpPost("/api/commande/fixe")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public new async Task<IActionResult> Fixe([FromQuery] ParamsFixeLigne paramsLigne)
        {
            return await base.Fixe(paramsLigne);
        }

        [HttpDelete("/api/commande/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Supprime([FromQuery] ParamsKeyLigne paramsLigne)
        {
            vérificateur.Initialise(paramsLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientOuFournisseur();
                CLFDocs contexte = await EtatSiteChangé();
                if (contexte != null)
                {
                    return Conflict(contexte);
                }
                await LigneExiste();
                CommandeModifiable();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.SupprimeLigne(vérificateur.LigneCLF);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour la ligne définie par la key.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/copie1")]
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
        [HttpPost("/api/commande/copieD")]
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
        [HttpPost("/api/commande/copieT")]
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
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/annule1")]
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
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type dont le AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/annuleD")]
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
        [HttpPost("/api/commande/annuleT")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleT(CLFDocsSynthèse clfDocs)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.Annule(docs, _type);
            return await Action(clfDocs, action);
        }

        #endregion

    }
}
