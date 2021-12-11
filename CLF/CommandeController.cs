﻿using KalosfideAPI.Data;
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
            _type = TypeClf.Commande;
        }

        #region Vérificateurs

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
            vérificateur.ArchiveProduit = produit.Archives.OrderBy(a => a.Date).Last();
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
                        // le fournisseur peut envoyer AFixer seul défini
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
            if (vérificateur.CLFLigne.TypeCommande != null
                && !TypeUnitéDeCommande.DemandeEstValide(vérificateur.CLFLigne.TypeCommande, vérificateur.ArchiveProduit.TypeCommande))
            {
                vérificateur.Erreur = RésultatBadRequest("typeCommande", "Type invalide");
                throw new VérificationException();
            }

            string code;
            if (vérificateur.CLFLigne.Quantité.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.CLFLigne.Quantité.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest("quantité", code);
                    throw new VérificationException();
                }
            }

            if (vérificateur.CLFLigne.AFixer.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.CLFLigne.AFixer.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest("aFixer", code);
                    throw new VérificationException();
                }
            }

        }

        #endregion

        #region Client-Lecture

        /// <summary>
        /// Lecture par un client de sa dernière commande si le paramètre ne contient pas de date.
        /// Vérification par un client que le catalogue chargé avec cette commande est toujours valide si le paramètre contient une date.
        /// 
        /// Si le site est d'état Catalogue, retourne une erreur Conflict contenant un ContexteCatalogue avec l'état Catalogue et une date égale à DateNulle.
        /// Pour une vérification, si le site est ouvert et si la date du paramètre est antérieure à celle du catalogue de la bdd,
        /// retourne une erreur Conflict contenant un ContexteCatalogue avec l'état Ouvert et la date du catalogue de la bdd.
        /// </summary>
        /// <param name="paramsKeyClient">key du client et date du catalogue de sa commande s'il l'a déjà chargée</param>
        /// <returns>un CLFDocs dont le champ Documents contient le CLFDoc de la dernière commande du client pour une lecture,
        /// un CLFDocs vide pour une vérification</returns>
        [HttpGet("/api/commande/enCours")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> EnCours([FromQuery] ParamsKeyClient paramsKeyClient)
        {
            vérificateur.Initialise(paramsKeyClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClient();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            if (paramsKeyClient.DateCatalogue.HasValue)
            {
                // la commande en cours a déjà été chargée par l'application client qui vérifie seulement le contexte catalogue
                // la vérification a réussi
                // retourne un CLFDocs vide
                return Ok(new CLFDocs());
            }

            CLFDocs docs = await _service.CommandeEnCours(paramsKeyClient);

            return Ok(docs);
        }

        #endregion

        #region Action-Commande

        /// <summary>
        /// Fixe la date du bon de commande du client défini par la clé.
        /// Retourne un CLFDoc contenant uniquement cette date.
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/envoi")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Envoi([FromQuery] ParamsKeyClient paramsClient)
        {
            vérificateur.Initialise(paramsClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveau();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            DocCLF dernièreCommande = await _service.DernierDoc(vérificateur.Client, TypeClf.Commande);
            // la dernière commande doit exister et ne pas être envoyée
            if (dernièreCommande == null || dernièreCommande.Date.HasValue)
            {
                return RésultatBadRequest("DerniereCommandeAbsenteOuEnvoyée");
            }

            RetourDeService<CLFDoc> retour = await _service.EnvoiCommande(vérificateur.Site, dernièreCommande);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Crée une nouvelle commande vide pour le client défini par la clé
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/nouveau")]
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
        /// Crée une nouvelle commande pour le client défini par la clé avec des détails copiés sur ceux de la commande
        /// précédente dont les produits sont toujours disponibles
        /// </summary>
        /// <param name="paramsClient">contient la clé du client et la date du catalogue</param>
        /// <returns></returns>
        [HttpPost("/api/commande/clone")]
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
        [HttpPost("/api/commande/efface")]
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

        #endregion

        #region Action-Ligne

        [HttpPost("/api/commande/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute([FromQuery] ParamsVide paramsLigne, CLFLigne ligne)
        {
            vérificateur.Initialise(ligne, paramsLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveauOuFournisseur();
                ContexteCatalogue();
                await DocExiste();
                LigneNExistePas();
                DocModifiable();
                await PeutCommanderProduit();
                ChampsInterditsAbsents();
                ChampsRequisPrésents();
                ChampsPrésentsValides();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }


            RetourDeService retour = await _service.AjouteLigneCommande(vérificateur.Site, ligne);

            return SaveChangesActionResult(retour);
        }

        [HttpPut("/api/commande/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite([FromQuery] ParamsVide paramsLigne, CLFLigne ligne)
        {
            vérificateur.Initialise(ligne, paramsLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveauOuFournisseur();
                ContexteCatalogue();
                await LigneExiste();
                DocModifiable();
                await PeutCommanderProduit();
                ChampsInterditsAbsents();
                ChampsPrésentsValides();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.EditeLigne(vérificateur.LigneCLF, ligne);

            return SaveChangesActionResult(retour);
        }

        [HttpPost("/api/commande/fixe")]
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

        [HttpDelete("/api/commande/supprime")]
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
        [HttpPost("/api/commande/copie1")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Copie1([FromQuery] KeyUidRnoNo2 keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.CopieQuantité(ligneCLF, TypeClf.Commande);
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
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieD([FromQuery] KeyUidRnoNo keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.CopieQuantité(docCLF, TypeClf.Commande);
            return await Action(keyDoc, action);
        }

        /// <summary>
        /// Copie la valeur de Quantité dans AFixer pour chaque ligne des documents d'un client dont le No est dans une liste
        /// dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs
        /// </summary>
        /// <param name="paramsSynthèse">a la clé du client et contient la liste des No des documents à synthétiser</param>
        /// <returns></returns>
        [HttpPost("/api/commande/copieT")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieT(ParamsSynthèse paramsSynthèse)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.CopieQuantité(docs, TypeClf.Commande);
            return await Action(paramsSynthèse, action);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/annule1")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Annule1([FromQuery] KeyUidRnoNo2 keyLigne)
        {
            Task<RetourDeService> action(LigneCLF ligneCLF) => _service.Annule(ligneCLF, TypeClf.Commande);
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
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleD([FromQuery] KeyUidRnoNo keyDoc)
        {
            Task<RetourDeService> action(DocCLF docCLF) => _service.Annule(docCLF, TypeClf.Commande);
            return await Action(keyDoc, action);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents d'un client dont le No est dans une liste
        /// </summary>
        /// <param name="paramsSynthèse">a la clé du client et contient la liste des No des documents à synthétiser</param>
        /// <returns></returns>
        [HttpPost("/api/commande/annuleT")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleT(ParamsSynthèse paramsSynthèse)
        {
            Task<RetourDeService> action(List<DocCLF> docs) => _service.Annule(docs, TypeClf.Commande);
            return await Action(paramsSynthèse, action);
        }

        #endregion

    }
}
