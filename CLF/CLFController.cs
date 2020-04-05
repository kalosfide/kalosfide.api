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

namespace KalosfideAPI.CLF
{
    public abstract class CLFController : BaseController
    {
        protected readonly ICLFService _service;
        protected readonly IUtileService _utile;
        protected readonly IUtilisateurService _utilisateurService;

        protected string _type;
        protected string _typeBon;
        protected readonly Vérificateur vérificateur;

        public CLFController(ICLFService service,
            IUtileService utile,
            IUtilisateurService utilisateurService)
        {
            _service = service;
            _utile = utile;
            _utilisateurService = utilisateurService;
            vérificateur = new Vérificateur();
        }

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="keySite">key du site</param>
        /// <returns></returns>
        protected async Task<IActionResult> Clients(KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (!await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            CLFDocs clfDocs = await _service.ClientsAvecBons(site, _type);

            return Ok(clfDocs);
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        protected async Task<IActionResult> Client(KeyUidRno keyClient)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                return Forbid();
            }

            CLFDocs commandes = await _service.BonsDUnClient(site, keyClient, _type);

            return Ok(commandes);
        }

        #region Vérifications

        /// <summary>
        /// Vérifie que le paramètre passé au vérificateur correspond à un client actif. Fixe le Client et le Site du vérificateur
        /// </summary>
        protected async Task ClientDeLAction()
        {
            bool filtreClient(Client c) => c.Uid == vérificateur.KeyClient.Uid && c.Rno == vérificateur.KeyClient.Rno;
            Client client = await _utile.ClientsAvecRoleEtSite(filtreClient, null, null).FirstOrDefaultAsync();
            if (client == null)
            {
                vérificateur.Erreur = NotFound();
                throw new VérificationException();
            }
            vérificateur.Client = client;

            Role role = client.Role;
            if (role.Etat == TypeEtatRole.Inactif || role.Etat == TypeEtatRole.Exclu)
            {
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }

            vérificateur.Site = role.Site;

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le fournisseur du site
        /// </summary>
        protected async Task UtilisateurEstFournisseur()
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }

            vérificateur.EstFournisseur = await carte.EstActifEtAMêmeUidRno(vérificateur.Site.KeyParam);
            if (!vérificateur.EstFournisseur)
            {
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client du document
        /// </summary>
        protected async Task UtilisateurEstClient()
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }

            vérificateur.EstClient = await carte.EstActifEtAMêmeUidRno(vérificateur.KeyClient.KeyParam);
            if (!vérificateur.EstClient)
            {
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }
        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client du document ou le fournisseur du site
        /// </summary>
        protected async Task UtilisateurEstClientOuFournisseur()
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                vérificateur.Erreur = Forbid();
                throw new VérificationException();
            }

            vérificateur.EstClient = await carte.EstActifEtAMêmeUidRno(vérificateur.KeyClient.KeyParam);
            if (!vérificateur.EstClient)
            {
                vérificateur.EstFournisseur = await carte.EstActifEtAMêmeUidRno(vérificateur.Site.KeyParam);
                if (!vérificateur.EstFournisseur)
                {
                    vérificateur.Erreur = Forbid();
                    throw new VérificationException();
                }
            }
            else
            {
                vérificateur.EstFournisseur = false;
            }

        }

        /// <summary>
        /// Lit le document définie par vérificateur.KeyDoc et vérifie qu'il existe. Fixe vérificateur.DocCLF.
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
        /// </summary>
        protected void DocEstASynthétiser()
        {
            DocCLF doc = vérificateur.DocCLF;
            bool peutEditerAFixer = doc.Date.HasValue && !doc.NoGroupe.HasValue;
            if (!peutEditerAFixer)
            {
                vérificateur.Erreur = RésultatBadRequest("AFixerNonModifiable");
                throw new VérificationException();
            }
        }

        #endregion

        #region Action

        protected async Task<IActionResult> Fixe(ParamsFixeLigne paramsFixeLigne)
        {
            vérificateur.Initialise(paramsFixeLigne);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
                await LigneExiste();
                DocEstASynthétiser();
                string code = QuantitéDef.Vérifie(paramsFixeLigne.AFixer);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest(code, "aFixer");
                    throw new VérificationException();
                }
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.FixeLigne(vérificateur.LigneCLF, paramsFixeLigne.AFixer);
            if (retour == null)
            {
                return RésultatBadRequest("RienAFaire");
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur la ligne définie par la clé. Retourne une erreur si l'action est impossible.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(KeyUidRnoNo2 keyLigne, Func<LigneCLF, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(keyLigne);
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

            RetourDeService retour = await action(vérificateur.LigneCLF);
            if (retour == null)
            {
                return RésultatBadRequest("RienAFaire");
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur chaque ligne du document défini par la clé. Retourne une erreur s'il n'y a pas d'action possible.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(KeyUidRnoNo keyDoc, Func<DocCLF, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(keyDoc);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
                await DocExiste();
                DocEstASynthétiser();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await action(vérificateur.DocCLF);
            if (retour == null)
            {
                return RésultatBadRequest("RienAFaire");
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur chaque ligne des documents dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs.
        /// Retourne une erreur si l'un des documents n'est pas à synthétiser.
        /// </summary>
        /// <param name="clfDocs">ne contient que la clé du client des CLFDoc réduits à leur No</param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Action(CLFDocsSynthèse clfDocs, Func<List<DocCLF>, Task<RetourDeService>> action)
        {
            vérificateur.Initialise(clfDocs.KeyClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            List<DocCLF> docs = await _service.DocumentsEnvoyésSansSynthèse(clfDocs, _typeBon);
            if (docs.Count != clfDocs.NoDocs.Count)
            {
                return RésultatBadRequest("DocumentsPasEnvoyésOuAvecSynthèse");
            }

            RetourDeService retour = await action(docs);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Exécute une action sur chaque ligne des documents dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs.
        /// Retourne une erreur si l'un des documents n'est pas à synthétiser.
        /// </summary>
        /// <param name="clfDocs">ne contient que la clé du client des CLFDoc réduits à leur No</param>
        /// <param name="action">retourne null si l'action est impossible</param>
        /// <returns></returns>
        protected async Task<IActionResult> Synthèse(CLFDocsSynthèse clfDocs)
        {
            vérificateur.Initialise(clfDocs.KeyClient);
            try
            {
                await ClientDeLAction();
                await UtilisateurEstFournisseur();
            }
            catch (VérificationException)
            {
                return vérificateur.Erreur;
            }

            List<DocCLF> docs = await _service.DocumentsEnvoyésSansSynthèse(clfDocs, _typeBon);
            if (docs.Count != clfDocs.NoDocs.Count)
            {
                return RésultatBadRequest("DocumentsPasEnvoyésOuAvecSynthèse");
            }

            RetourDeService<DocCLF> retour = await _service.Synthèse(docs, _type);

            if (retour.Ok)
            {
                return Ok(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        #endregion
    }
}
