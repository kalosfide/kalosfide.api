using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KalosfideAPI.Catalogues;

namespace KalosfideAPI.CLF
{
    public abstract class CLFController : AvecCarteController
    {
        protected readonly ICLFService _service;
        protected readonly IUtileService _utile;

        /// <summary>
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        protected TypeCLF _type;
        protected readonly Vérificateur vérificateur;

        public CLFController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService) : base(utilisateurService)
        {
            _service = service;
            _utile = utile;
            vérificateur = new Vérificateur();
        }

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="idSite">Id du site</param>
        /// <returns></returns>
        protected async Task<IActionResult> Clients(uint idSite)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(idSite, PermissionsEtatRole.Actif);
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            vérificateur.Site = carte.Fournisseur.Site;
            try
            {
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }


            CLFDocs clfDocs = await _service.ClientsAvecBons(idSite, _type);

            return Ok(clfDocs);
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="idClient"></param>
        /// <returns></returns>
        protected async Task<IActionResult> Client(uint idClient)
        {
            vérificateur.Initialise(idClient);
            try
            {
                // seuls les clients actifs peuvent faire l'objet d'une synthèse
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            CLFDocs clfDocs = await _service.BonsDUnClient(vérificateur.Site, idClient, _type);

            return Ok(clfDocs);
        }

        #endregion Lecture

        #region Vérifications

        /// <summary>
        /// Vérifie que le paramètre passé au vérificateur correspond à un client d'état non Fermé. Fixe le Client et le Site du vérificateur
        /// </summary>
        protected async Task ClientDeLAction()
        {
            Client client = await _service.ClientAvecSite(vérificateur.IdClient);
            if (client == null)
            {
                vérificateur.Erreur = NotFound();
                throw new VérificationException();
            }

            if (client.Etat == EtatRole.Fermé)
            {
                vérificateur.Erreur = RésultatBadRequest("Client fermé");
                throw new VérificationException();
            }

            vérificateur.Client = client;
            vérificateur.Site = client.Site;

        }

        /// <summary>
        /// Vérifie que le paramètre passé au vérificateur correspond à un client d'état autorisé. Fixe le Client et le Site du vérificateur.
        /// </summary>
        /// <param name="permissions">PermissionsEtatRole définissant les états autorisés</param>
        /// <returns></returns>
        protected async Task ClientDeLAction(PermissionsEtatRole permissions)
        {
            Client client = await _service.ClientAvecSite(vérificateur.IdClient);
            if (client == null)
            {
                vérificateur.Erreur = NotFound();
                throw new VérificationException();
            }

            if (!permissions.Permet(client.Etat))
            {
                vérificateur.Erreur = RésultatBadRequest("Etat du client interdit: {0}", nameof(client.Etat));
                throw new VérificationException();
            }

            vérificateur.Client = client;
            vérificateur.Site = client.Site;

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le fournisseur du site
        /// </summary>
        protected async Task UtilisateurEstFournisseur()
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(vérificateur.Site.Id, PermissionsEtatRole.Actif);
            if (carte.Erreur != null)
            {
                vérificateur.Erreur = carte.Erreur;
                throw new VérificationException();
            }
        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client du document et que son role est actif ou nouveau
        /// </summary>
        protected async Task UtilisateurEstClientActifOuNouveau()
        {
            CarteUtilisateur carte = await CréeCarteClientDeClient(vérificateur.IdClient, PermissionsEtatRole.Actif, PermissionsEtatRole.PasInactif);
            if (carte.Erreur != null)
            {
                vérificateur.Erreur = carte.Erreur;
                throw new VérificationException();
            }
        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client du document et que son role est actif ou nouveau ou qu'il est le fournisseur du site
        /// </summary>
        protected async Task UtilisateurEstClientActifOuNouveauOuFournisseur()
        {
            CarteUtilisateur carte = await CréeCarteClientDeClientOuFournisseurDeSite(vérificateur.IdClient, vérificateur.Site.Id,
                PermissionsEtatRole.Actif, PermissionsEtatRole.PasInactif);
            if (carte.Erreur != null)
            {
                vérificateur.Erreur = carte.Erreur;
                throw new VérificationException();
            }

            vérificateur.Fournisseur = carte.Fournisseur;
            vérificateur.EstClient = carte.Fournisseur == null;

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client du document et que son role est actif ou nouveau ou qu'il est le fournisseur du site
        /// </summary>
        protected async Task UtilisateurEstClientPasFerméOuFournisseur()
        {
            CarteUtilisateur carte = await CréeCarteClientDeClientOuFournisseurDeSite(vérificateur.IdClient, vérificateur.Site.Id,
                PermissionsEtatRole.Actif, PermissionsEtatRole.PasFermé);
            if (carte.Erreur != null)
            {
                vérificateur.Erreur = carte.Erreur;
                throw new VérificationException();
            }

            vérificateur.Fournisseur = carte.Fournisseur;
            vérificateur.EstClient = carte.Fournisseur == null;
        }

        /// <summary>
        /// Vérifie que la modification du catalogue n'est pas en cours et, si une date a été passée dans le paramètre
        /// que la modification du catalogue n'a pas eu lieu depuis cette date.
        /// Si la vérification échoue, retourne une erreur 409 Conflict contenant
        /// </summary>
        /// <returns></returns>
        protected void ContexteCatalogue()
        {
            if (!vérificateur.Site.Ouvert
                || (vérificateur.DateCatalogue.HasValue && vérificateur.DateCatalogue.Value < vérificateur.Site.DateCatalogue))
            { 
                vérificateur.Erreur = Conflict(new ContexteCatalogue(vérificateur.Site));
                throw new VérificationException();
            }
        }

        /// <summary>
        /// Lit le document défini par vérificateur.KeyDoc et vérifie qu'il existe. Fixe vérificateur.DocCLF avec ses lignes.
        /// </summary>
        protected async Task DocExiste()
        {
            DocCLF docCLF = await _service.DocCLFDeKey(vérificateur.KeyDoc, _type);
            if (docCLF == null)
            {
                vérificateur.Erreur = RésultatBadRequest("DocNExistePas");
                throw new VérificationException();
            }
            vérificateur.DocCLF = docCLF;
        }

        /// <summary>
        /// Lit le document défini par vérificateur.KeyDoc et vérifie qu'il existe. Fixe vérificateur.DocCLF avec ses lignes.
        /// </summary>
        protected async Task BonExiste()
        {
            DocCLF docCLF = await _service.DocCLFDeKey(vérificateur.KeyDoc, DocCLF.TypeBon(_type));
            if (docCLF == null)
            {
                vérificateur.Erreur = RésultatBadRequest("DocNExistePas");
                throw new VérificationException();
            }
            vérificateur.DocCLF = docCLF;
        }

        /// <summary>
        /// Lit la ligne définie par vérificateur.KeyLigne et vérifie qu'elle existe. Fixe vérificateur.LigneCLF et vérificateur.DocCLF.
        /// </summary>
        protected async Task LigneExiste()
        {
            vérificateur.LigneCLF = await _service.LigneCLFDeKey(vérificateur.KeyLigne, _type);
            if (vérificateur.LigneCLF == null)
            {
                vérificateur.Erreur = RésultatBadRequest("DétailNExistePas");
                throw new VérificationException();
            }
            vérificateur.DocCLF = vérificateur.LigneCLF.Doc;
        }

        /// <summary>
        /// Vérifie que vérificateur.DocCLF est envoyé et sans synthèse.
        /// Un bon virtuel est considéré comme envoyé et sans synthèse.
        /// </summary>
        protected void DocEstASynthétiser()
        {
            DocCLF doc = vérificateur.DocCLF;
            bool peutEditerAFixer = doc.No == 0 || (doc.Date.HasValue && !doc.NoGroupe.HasValue);
            if (!peutEditerAFixer)
            {
                vérificateur.Erreur = RésultatBadRequest("AFixerNonModifiable");
                throw new VérificationException();
            }
        }

        /// <summary>
        /// Vérifie que vérificateur.DocCLF peut être édité ou supprimé et que l'utilisateur en a le droit.
        /// </summary>
        protected void DocModifiable()
        {
            DocCLF doc = vérificateur.DocCLF;
            if (vérificateur.EstClient)
            {
                // Le client peut modifier un bon de commande qui n'a pas été envoyé (i.e. sans Date) sauf le bon virtuel
                if (doc.Date.HasValue || vérificateur.KeyDoc.No == 0)
                {
                    vérificateur.Erreur = RésultatBadRequest("CommandeEnvoyéeOuVirtuelle");
                    throw new VérificationException();
                }
            }
            else
            {
                // Le fournisseur ne peut modifier que les bons de commande ou de livraison virtuels
                if (doc.Type == TypeCLF.Facture)
                {
                    vérificateur.Erreur = RésultatBadRequest("ModifieFacture");
                    throw new VérificationException();
                }
                if (doc.No != 0)
                {
                    vérificateur.Erreur = RésultatBadRequest("BonNonVirtuel");
                    throw new VérificationException();
                }
            }
        }

        #endregion

        #region Action

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramsCréeBon"></param>
        /// <returns></returns>
        protected async Task<IActionResult> CréeBon(ParamsCréeBon paramsCréeBon)
        {
            vérificateur.Initialise(paramsCréeBon);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveauOuFournisseur();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            uint noBon;
            DocCLF docACopier = null;
            if (vérificateur.EstClient)
            {
                if (_type != TypeCLF.Commande)
                {
                    return RésultatInterdit("Un client ne peut créer que des bons de commande.");
                }
                DocCLF dernièreCommande = await _service.DernierDoc(vérificateur.IdClient, TypeCLF.Commande);
                if (vérificateur.Client.Etat == EtatRole.Nouveau && dernièreCommande != null)
                {
                    return RésultatInterdit("Un client d'état Nouveau ne peut créer qu'un seul bon de commande");
                }
                if (paramsCréeBon.Copie == true)
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
                noBon = dernièreCommande == null ? 1 : dernièreCommande.No + 1;
            }
            else
            {
                // le client doit être d'état Actif
                if (vérificateur.Client.Etat != EtatRole.Actif)
                {
                    return RésultatBadRequest("ClientInactif");
                }
                // key du bon virtuel du client
                KeyDocSansType key = new KeyDocSansType
                {
                    Id = vérificateur.IdClient,
                    No = 0
                };
                DocCLF bonVirtuel = await _service.DocCLFDeKey(key, _type);
                if (bonVirtuel != null)
                {
                    // il ne peut y avoir qu'un seul bon virtuel d'un type donné
                    return RésultatBadRequest("BonVirtuelPrésent");
                }
                if (paramsCréeBon.Copie == true)
                {
                    // on ne peut copier les lignes que si la synthèse précédente a été réalisée à partir du seul bon virtuel
                    // c'est le cas s'il n'y a pas de bon ayant pour NoGroupe le No de cette synthèse
                    docACopier = await _service.DernierDoc(vérificateur.IdClient, DocCLF.TypeSynthèse(_type));
                    if (docACopier == null)
                    {
                        return RésultatBadRequest("PasDeDernièreSynthèse");
                    }
                    if (!(await _service.EstSynthèseSansBons(docACopier)))
                    {
                        return RésultatBadRequest("DernièreSynthèseAvecBons");
                    }
                }
                noBon = 0;
            }

            RetourDeService<DocCLF> retour = await _service.AjouteBon(vérificateur.IdClient, _type, noBon);

            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            if (docACopier != null)
            {
                RetourDeService retour1 = await _service.CopieLignes(retour.Entité, docACopier);
                if (!retour1.Ok)
                {
                    return SaveChangesActionResult(retour1);
                }
            }

            return RésultatCréé(CLFDoc.DeNo(noBon));
        }

        /// <summary>
        /// Efface toutes les lignes du bon et si le bon est virtuel, supprime le bon.
        /// </summary>
        /// <param name="paramsBon"></param>
        /// <returns></returns>
        public async Task<IActionResult> EffaceBon([FromQuery] ParamsKeyDoc paramsBon)
        {
            vérificateur.Initialise(paramsBon);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveauOuFournisseur();
                await BonExiste();
                DocModifiable();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            if (!vérificateur.EstClient)
            {
                // le client doit être d'état Actif
                if (vérificateur.Client.Etat != EtatRole.Actif)
                {
                    return RésultatBadRequest("ClientInactif");
                }
            }

            RetourDeService retour = await _service.EffaceBonEtSupprimeSiVirtuel(vérificateur.DocCLF);

            return SaveChangesActionResult(retour);
        }

        public async Task<IActionResult> Supprime([FromQuery] ParamsSupprimeLigne paramsSupprime)
        {
            vérificateur.Initialise(paramsSupprime);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstClientActifOuNouveauOuFournisseur();
                ContexteCatalogue();
                await DocExiste();
                DocModifiable();
                // vérifie que la ligne commandant le produit dont le No est le No2 du paramétre existe et fixe vérificateur.LigneCLF
                LigneCLF ligne = vérificateur.DocCLF.Lignes.Where(l => l.ProduitId == paramsSupprime.ProduitId).FirstOrDefault();
                if (ligne == null)
                {
                    return NotFound();
                }
                vérificateur.LigneCLF = ligne;
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            if (!vérificateur.EstClient)
            {
                // le client doit être d'état Actif
                if (vérificateur.Client.Etat != EtatRole.Actif)
                {
                    return RésultatBadRequest("ClientInactif");
                }
            }

            RetourDeService retour = await _service.SupprimeLigne(vérificateur.LigneCLF);

            return SaveChangesActionResult(retour);
        }


        protected async Task<IActionResult> Fixe(ParamsFixeLigne paramsFixeLigne)
        {
            vérificateur.Initialise(paramsFixeLigne);
            try
            {
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
                await LigneExiste();
                DocEstASynthétiser();
                string code = QuantitéDef.Vérifie(paramsFixeLigne.AFixer);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest("aFixer", code);
                    throw new VérificationException();
                }
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.FixeLigne(vérificateur.LigneCLF, paramsFixeLigne.AFixer);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur la ligne définie par la clé. Retourne une erreur si l'action est impossible.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(KeyLigneSansType keyLigne, Func<LigneCLF, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(keyLigne);
            try
            {
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
                await LigneExiste();
                DocEstASynthétiser();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await action(vérificateur.LigneCLF);

            if (retour == null)
            {
                return RésultatBadRequest("");
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur chaque ligne du document défini par la clé. Retourne une erreur s'il n'y a pas de ligne où l'action est possible.
        /// </summary>
        /// <param name="keyDocSansType"></param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(KeyDocSansType keyDocSansType, Func<DocCLF, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(keyDocSansType);
            try
            {
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
                await DocExiste();
                DocEstASynthétiser();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await action(vérificateur.DocCLF);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur chaque ligne des documents d'un client dont le No est dans une liste.
        /// Retourne une erreur si l'un des documents n'est pas à synthétiser.
        /// </summary>
        /// <param name="paramsSynthèse">contient l'Id du client et la liste des No des documents à synthétiser</param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(ParamsSynthèse paramsSynthèse, Func<List<DocCLF>, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(paramsSynthèse.Id);
            try
            {
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            // Lit la liste des documents synthétisables dont le No est dans la liste.
            List<DocCLF> docs = await _service.DocumentsEnvoyésSansSynthèse(paramsSynthèse, DocCLF.TypeBon(_type));
            if (docs.Count != paramsSynthèse.NoDocs.Count)
            {
                // L'un des No de la liste ne correspond pas à un document synthétisable.
                return RésultatBadRequest("DocumentsPasEnvoyésOuAvecSynthèse");
            }

            RetourDeService retour = await action(docs);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Crée un document de synthèse à partir des documents d'un client dont le No est dans une liste.
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// Retourne une erreur si l'un des documents n'est pas à synthétiser.
        /// </summary>
        /// <param name="paramsSynthèse">contient l'Id du client et la liste des No des documents à synthétiser</param>
        /// <returns></returns>
        protected async Task<IActionResult> Synthèse(ParamsSynthèse paramsSynthèse)
        {
            vérificateur.Initialise(paramsSynthèse.Id);
            try
            {
                await ClientDeLAction(PermissionsEtatRole.Actif);
                await UtilisateurEstFournisseur();
                ContexteCatalogue();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            if (vérificateur.Client.Etat != EtatRole.Actif)
            {

            }

            if (paramsSynthèse.NoDocs == null || paramsSynthèse.NoDocs.Count == 0)
            {
                return RésultatBadRequest("PasDeNosDeDocuments");
            }

            // Lit la liste des documents synthétisables dont le No est dans la liste.
            List<DocCLF> docs = await _service.DocumentsEnvoyésSansSynthèse(paramsSynthèse, DocCLF.TypeBon(_type));
            if (docs.Count != paramsSynthèse.NoDocs.Count)
            {
               // L'un des No de la liste ne correspond pas à un document synthétisable.
               return RésultatBadRequest("DocumentsPasEnvoyésOuAvecSynthèse");
            }

            RetourDeService<DocCLF> retour = await _service.Synthèse(vérificateur.Site, paramsSynthèse.Id, docs, _type);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        #endregion
    }
}
