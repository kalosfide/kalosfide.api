using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class CommandeController : BaseController
    {
        private readonly ICommandeService _service;
        private readonly IUtileService _utile;
        private readonly IUtilisateurService _utilisateurService;
        private readonly IDétailCommandeService _détailCommandeService;

        public CommandeController(ICommandeService service,
            IUtileService utile,
            IUtilisateurService utilisateurService,
            IDétailCommandeService détailCommandeService)
        {
            _service = service;
            _utile = utile;
            _utilisateurService = utilisateurService;
            _détailCommandeService = détailCommandeService;
        }

        #region Client-Lecture

        /// <summary>
        /// retourne un CommandeVue contenant la dernière Commande d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns>CommandeVue</returns>
        [HttpGet("/api/commande/enCours")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> EnCours([FromQuery] KeyUidRno keyClient)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (!await carte.EstActifEtAMêmeUidRno(keyClient.KeyParam))
            {
                return Forbid();
            }

            CommandeVue commandeVue = await _service.EnCours(keyClient);
            if (commandeVue == null)
            {
                return NotFound();
            }

            return Ok(commandeVue);
        }

        /// <summary>
        /// retourne un ContexteCommande contenant les données d'état définissant les droits
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns></returns>
        [HttpGet("/api/commande/contexte")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Contexte([FromQuery] KeyUidRno keyClient)
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

            return Ok(await _service.Contexte(keyClient, site));
        }

        /// <summary>
        /// retourne le DétailCommandeVue de la clé
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns>CommandeVue</returns>
        [HttpGet("/api/commande/lit")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Lit([FromQuery] KeyUidRnoNo2 keyDétail)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            bool estLeClient = await carte.EstActifEtAMêmeUidRno(AKeyUidRnoNo2.KeyUidRno_1(keyDétail).KeyParam);
            if (!estLeClient)
            {
                bool estFournisseur = await carte.EstActifEtAMêmeUidRno(AKeyUidRnoNo2.KeyUidRno_2(keyDétail).KeyParam);
                if (!estFournisseur)
                {
                    return Forbid();
                }
            }

            DétailCommande détail = await _détailCommandeService.Lit(keyDétail.KeyParam);
            if (détail == null)
            {
                return NotFound();
            }

            DétailCommandeVue vue = _détailCommandeService.CréeVue(détail);
            return Ok(vue);
        }

        #endregion

        #region Action-Vérificateurs

        /// <summary>
        /// erreur retournée pour indiquer à l'UI qu'il faut recharger la CommanderVue
        /// quand le site est fermé
        /// quand le catalogue est plus récent que la commande
        /// quand la commande n'est pas d'état nouveau
        /// </summary>
        /// <returns></returns>
        private IActionResult RésultatEtatChangé()
        {
            return RésultatBadRequest("EtatChange");
        }

        /// <summary>
        /// Vérifie que le paramètre passé au vérificateur correspond à un client actif. Fixe le Client et le Site du vérificateur
        /// </summary>
        /// <param name="paramClient"></param>
        /// <returns></returns>
        private async Task ClientDeLAction(Vérificateur vérificateur)
        {
            bool filtreClient(Client c) => c.Uid == vérificateur.KeyClient.Uid && c.Rno == vérificateur.KeyClient.Rno;
            Client client = await _utile.ClientsAvecRoleEtSite(filtreClient, null, null).FirstOrDefaultAsync();
            if (client == null)
            {
                vérificateur.Erreur = NotFound();
                return;
            }
            vérificateur.Client = client;

            Role role = client.Role;
            if (role.Etat == TypeEtatRole.Inactif || role.Etat == TypeEtatRole.Exclu)
            {
                vérificateur.Erreur = Forbid();
                return;
            }

            vérificateur.Site = role.Site;

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client de la commande ou le fournisseur du site
        /// </summary>
        /// <param name="paramClient"></param>
        /// <returns></returns>
        private async Task UtilisateurEstClientOuFournisseur(Vérificateur vérificateur)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                vérificateur.Erreur = Forbid();
                return;
            }

            vérificateur.EstClient = await carte.EstActifEtAMêmeUidRno(vérificateur.KeyClient.KeyParam);
            if (!vérificateur.EstClient)
            {
                vérificateur.EstFournisseur = await carte.EstActifEtAMêmeUidRno(vérificateur.Site.KeyParam);
                if (!vérificateur.EstFournisseur)
                {
                    vérificateur.Erreur = Forbid();
                    return;
                }
            }
            else
            {
                vérificateur.EstFournisseur = false;
            }

        }

        /// <summary>
        /// Vérifie que l'utilisateur est le client de la commande ou le fournisseur du site
        /// </summary>
        /// <param name="paramClient"></param>
        /// <returns></returns>
        private async Task UtilisateurEstFournisseur(Vérificateur vérificateur)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                vérificateur.Erreur = Forbid();
                return;
            }

            vérificateur.EstFournisseur = await carte.EstActifEtAMêmeUidRno(vérificateur.Site.KeyParam);
            if (!vérificateur.EstFournisseur)
            {
                vérificateur.Erreur = Forbid();
                return;
            }

        }

        /// <summary>
        /// si l'utilisateur est un client, vérifie qu'il n'y a pas eu de livraison ou de modification de l'état du site ou du catalogue
        /// depuis que l'utilisateur a chargé les données
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private async Task EtatSiteChangé(Vérificateur vérificateur)
        {
            // cas d'un client
            if (vérificateur.EstClient)
            {
            }
                // vérifie que le site est ouvert
                if (vérificateur.Site.Etat != TypeEtatSite.Ouvert)
                {
                    vérificateur.Erreur = RésultatEtatChangé();
                    return;
                }

                // vérifie que la catalogue n'est pas plus récent que la commande
                DateTime date = await _utile.DateCatalogue(vérificateur.Site);
                if (vérificateur.DateCatalogue < date)
                {
                    vérificateur.Erreur = RésultatEtatChangé();
                    return;
                }
        }

        private async Task DerniereCommandeAbsenteOuEnvoyée(Vérificateur vérificateur)
        {
            Commande dernièreCommande = await _service.DernièreCommande(vérificateur.KeyClient);
            if (!(dernièreCommande == null || dernièreCommande.Date.HasValue))
            {
                vérificateur.Erreur = RésultatBadRequest("DerniereCommandePrésenteEtPasEnvoyée");
            }
            vérificateur.DernièreCommande = dernièreCommande;
        }

        private async Task DerniereCommandePrésenteEtEnvoyée(Vérificateur vérificateur)
        {
            Commande dernièreCommande = await _service.DernièreCommande(vérificateur.KeyClient);
            if (dernièreCommande == null || !dernièreCommande.Date.HasValue)
            {
                vérificateur.Erreur = RésultatBadRequest("DerniereCommandeAbsenteOuPasEnvoyée");
            }
            vérificateur.DernièreCommande = dernièreCommande;
        }

        /// <summary>
        /// Lit la commande définie par vérificateur.KeyCommande et vérifie qu'elle existe. Fixe vérificateur.Commande.
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private async Task CommandeExiste(Vérificateur vérificateur)
        {
            AKeyUidRnoNo key = vérificateur.KeyCommande;
            Func<Commande, bool> filtre = (Commande c) => c.Uid == key.Uid && c.Rno == key.Rno && c.No == key.No;
            Commande commande = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtre, null, null, null).FirstOrDefaultAsync();
            if (commande == null)
            {
                vérificateur.Erreur = RésultatBadRequest("CommandeNExistePas");
                return;
            }
            vérificateur.Commande = commande;
        }

        /// <summary>
        /// Lit le détail défini par vérificateur.KeyDétail et vérifie qu'il existe. Fixe vérificateur.Détail et vérificateur.Commande.
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private async Task DétailExiste(Vérificateur vérificateur)
        {
            vérificateur.Détail = await _détailCommandeService.Détail(vérificateur.KeyDétail);
            if (vérificateur.Détail == null)
            {
                vérificateur.Erreur = RésultatBadRequest("DétailNExistePas");
            }
            vérificateur.Commande = vérificateur.Détail.Commande;
        }

        /// <summary>
        /// Vérifie que la date de vérificateur.Commande est absente si l'utilisateur est le client et DateNulle.Date si l'utilisateur est le fournisseur
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task CommandeModifiable(Vérificateur vérificateur)
        {
            Commande commande = vérificateur.Commande;
            if ((vérificateur.EstClient && commande.Date.HasValue) || commande.Date != DateNulle.Date)
            {
                vérificateur.Erreur = RésultatBadRequest("CommandeNonModifiable");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Vérifie que vérificateur.Commande est dans une livraison sans date
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task ALivrerModifiable(Vérificateur vérificateur)
        {
            Commande commande = vérificateur.Commande;
            bool peutEditerALivrer = commande.Date.HasValue && commande.LivraisonNo.HasValue && !commande.Livraison.Date.HasValue;
            if (!peutEditerALivrer)
            {
                vérificateur.Erreur = RésultatBadRequest("AlivrerNonModifiable");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Vérifie que vérificateur.Commande.Détails ne contient pas déjà le détail
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task DétailNExistePas(Vérificateur vérificateur)
        {
            List<DétailCommande> détails = new List<DétailCommande>(vérificateur.Commande.Détails);
            DétailCommande détail = détails.Find(d => d.No2 == vérificateur.KeyDétail.No2);
            if (détail != null)
            {
                vérificateur.Erreur = RésultatBadRequest("DétailExiste");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// vérifie que le produit demandé par un détail existe, est disponible et appartient au site du client
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private async Task PeutCommanderProduit(Vérificateur vérificateur)
        {
            Produit produit = await _utile.Produit(vérificateur.Site, vérificateur.KeyDétail.No2);
            if (produit == null || produit.Etat != TypeEtatProduit.Disponible)
            {
                vérificateur.Erreur = RésultatBadRequest("Produit");
                return;
            }
            vérificateur.Produit = produit;
        }

        /// <summary>
        /// lit le détail s'il existe
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private async Task LitDétail(Vérificateur vérificateur)
        {
            vérificateur.Détail = await _détailCommandeService.Lit(vérificateur.KeyDétail.KeyParam);
        }

        private Task ChampsConformesAEtatCommande(Vérificateur vérificateur)
        {
            DétailCommandeVue détail = vérificateur.VueDétail;
            Commande commande = vérificateur.Commande;
            bool peutAjouterEtSupprimerDétailEtEditerDemande;
            bool peutEditerALivrer;
            if (vérificateur.EstClient)
            {
                peutAjouterEtSupprimerDétailEtEditerDemande = !commande.Date.HasValue;
                peutEditerALivrer = false;
            }
            else
            {
                peutAjouterEtSupprimerDétailEtEditerDemande = commande.Date.HasValue && commande.Date.Value.Ticks == 0
                    && (!commande.LivraisonNo.HasValue || !commande.Livraison.FactureNo.HasValue);
                peutEditerALivrer = commande.Date.HasValue && commande.LivraisonNo.HasValue && !commande.Livraison.Date.HasValue;
            }

            bool peutEditer = peutAjouterEtSupprimerDétailEtEditerDemande || peutEditerALivrer;
            if (!peutEditer)
            {
                vérificateur.Erreur = RésultatBadRequest("DétailNonEditable");
                return Task.CompletedTask;
            }
            if (!peutEditerALivrer && détail.ALivrer.HasValue)
            {
                vérificateur.Erreur = RésultatBadRequest("ALivrerNonEditable");
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Vérifie que les champs requis sont présents dans la vue.
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task ChampsRequisPrésents(Vérificateur vérificateur)
        {
            bool ajout = vérificateur.Détail == null;
            if (ajout)
            {
                if (!vérificateur.VueDétail.Demande.HasValue)
                {
                    if (vérificateur.EstClient)
                    {
                        vérificateur.Erreur = RésultatBadRequest("DemandeRequis");
                    }
                    else
                    {
                        // le fournisseur peut envoyer ALivrer seul défini
                        if (!vérificateur.VueDétail.ALivrer.HasValue)
                        {
                            vérificateur.Erreur = RésultatBadRequest("DemandeOuALivrerRequis");
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// vérifie que les champs qui vont être modifiés peuvent l'être
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task ChampsInterditsAbsents(Vérificateur vérificateur)
        {
            if (vérificateur.VueDétail.AFacturer.HasValue)
            {
                vérificateur.Erreur = RésultatBadRequest("AFacturerPrésent");
                return Task.CompletedTask;
            }
            if (vérificateur.EstClient)
            {
                if (vérificateur.VueDétail.ALivrer.HasValue)
                {
                    vérificateur.Erreur = RésultatBadRequest("ALivrerPrésent");
                }
            }
            else
            {
                if (vérificateur.VueDétail.ALivrer.HasValue)
                {
                    // le fournisseur ne peut fixer le ALivrer que
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// vérifie que les valeurs des champs présents sont valides
        /// </summary>
        /// <param name="vérificateur"></param>
        /// <returns></returns>
        private Task ChampsPrésentsValides(Vérificateur vérificateur)
        {
            if (vérificateur.VueDétail.TypeCommande != null && !TypeUnitéDeCommande.DemandeEstValide(vérificateur.VueDétail.TypeCommande, vérificateur.Produit.TypeCommande))
            {
                vérificateur.Erreur = RésultatBadRequest("invalide", "typeCommande");
                return Task.CompletedTask;
            }

            string code;
            if (vérificateur.VueDétail.Demande.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.VueDétail.Demande.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest(code, "demande");
                    return Task.CompletedTask;
                }
            }

            if (vérificateur.VueDétail.ALivrer.HasValue)
            {
                code = QuantitéDef.Vérifie(vérificateur.VueDétail.ALivrer.Value);
                if (code != null)
                {
                    vérificateur.Erreur = RésultatBadRequest(code, "aServir");
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Action-Commande

        /// <summary>
        /// crée une nouvelle commande vide pour le client défini par la clé
        /// </summary>
        /// <param name="paramsCrée"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/nouveau")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Nouveau([FromQuery] ParamsCréeCommande paramsCrée)
        {
            Vérificateur vérificateur = new Vérificateur(paramsCrée);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                DerniereCommandeAbsenteOuEnvoyée
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            long noCommande = vérificateur.DernièreCommande == null ? 1 : vérificateur.DernièreCommande.No + 1;
            RetourDeService retour = await _service.AjouteCommande(vérificateur.KeyClient, noCommande, vérificateur.Site, vérificateur.EstFournisseur);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// crée une nouvelle commande pour le client défini par la clé avec des détails copiés sur ceux de la commande
        /// précédente dont les produits sont toujours disponibles
        /// </summary>
        /// <param name="keyCommande">key de la commande ouverte</param>
        /// <returns></returns>
        [HttpPost("/api/commande/copie")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieBon([FromQuery] ParamsCréeCommande paramsCrée)
        {
            Vérificateur vérificateur = new Vérificateur(paramsCrée);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                DerniereCommandePrésenteEtEnvoyée
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }


            long noCommande = vérificateur.DernièreCommande == null ? 1 : vérificateur.DernièreCommande.No + 1;
            RetourDeService<Commande> retour = await _service.AjouteCommande(vérificateur.KeyClient, noCommande, vérificateur.Site, vérificateur.EstFournisseur);

            if (retour.Ok)
            {
                await _détailCommandeService.AjouteCopiesDétails(vérificateur.DernièreCommande);
            }

            return SaveChangesActionResult(retour);
        }


        /// <summary>
        /// Supprime les détails de la commande créés par l'utilisateur. S'il reste des détails, fixe leur ALivrer à 0.
        /// S'il n'y a plus de détails, supprime la commande.
        /// </summary>
        /// <param name="paramsSupprime"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/efface")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> SupprimeOuRefuse([FromQuery] ParamsSupprimeCommande paramsSupprime)
        {
            Vérificateur vérificateur = new Vérificateur(paramsSupprime);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                CommandeExiste,
                CommandeModifiable
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.SupprimeOuRefuse(vérificateur.Commande, vérificateur.EstClient);

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Action-DétailCommande

        [HttpPost("/api/commande/ajoute")]
        [ProducesResponseType(201)] // created
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Ajoute([FromQuery] ParamsEditeDétail paramsDétail, DétailCommandeVue vue)
        {
            Vérificateur vérificateur = new Vérificateur(vue, paramsDétail);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                CommandeExiste,
                DétailNExistePas,
                CommandeModifiable,
                PeutCommanderProduit,
                ChampsInterditsAbsents,
                ChampsRequisPrésents,
                ChampsPrésentsValides
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            RetourDeService<DétailCommande> retour = await _détailCommandeService.Ajoute(vue);

            if (retour.Ok)
            {
                return CreatedAtAction(nameof(LitDétail), vérificateur.KeyDétail, retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        [HttpPut("/api/commande/edite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Edite([FromQuery] ParamsEditeDétail paramsDétail, DétailCommandeVue vue)
        {
            Vérificateur vérificateur = new Vérificateur(vue, paramsDétail);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                DétailExiste,
                ChampsConformesAEtatCommande,
                PeutCommanderProduit,
                ChampsInterditsAbsents,
                ChampsPrésentsValides
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            RetourDeService<DétailCommande> retour = await _détailCommandeService.Edite(vérificateur.Détail, vue);

            return SaveChangesActionResult(retour);
        }

        [HttpDelete("/api/commande/supprime")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Supprime([FromQuery] ParamsSupprimeDétail paramsDétail)
        {
            Vérificateur vérificateur = new Vérificateur(paramsDétail);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstClientOuFournisseur,
                EtatSiteChangé,
                DétailExiste,
                CommandeModifiable
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _détailCommandeService.Supprime(vérificateur.Détail);

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Fournisseur-Action-Détail

        /// <summary>
        /// copie la Demande d'un détail d'une commande d'état APréparer sans numéro de livraison dans son ALivrer
        /// </summary>
        /// <param name="keyDétail"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/copieDem")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieDem([FromQuery] KeyUidRnoNo2 keyDétail)
        {
            Vérificateur vérificateur = new Vérificateur(keyDétail);
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstFournisseur,
                DétailExiste,
                ALivrerModifiable
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }

            RetourDeService retour = await _service.CopieDemandes(vérificateur.Site, keyDétail);
            if (retour == null)
            {
                return RésultatBadRequest("RienACopier");
            }

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Fournisseur-Action-Commande

        /// <summary>
        /// copie les demandes des détails d'une commande dans les aServir correspondants
        /// </summary>
        /// <param name="keyClient">CommandeVue contenant seulement l'état</param>
        /// <returns></returns>
        [HttpPost("/api/commande/copieDemC")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieDemC([FromQuery] KeyUidRno keyClient)
        {
            Vérificateur vérificateur = new Vérificateur
            {
                KeyClient = keyClient
            };
            await vérificateur.Vérifie(
                ClientDeLAction,
                UtilisateurEstFournisseur,
                DétailExiste,
                ALivrerModifiable
                );
            if (vérificateur.Erreur != null)
            {
                return vérificateur.Erreur;
            }
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            bool estFournisseur = await carte.EstActifEtAMêmeUidRno(site.KeyParam);
            if (!estFournisseur)
            {
                return Forbid();
            }

            RetourDeService retour = await _service.CopieDemandes(site, keyClient);
            if (retour == null)
            {
                return RésultatBadRequest("RienACopier");
            }

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Fournisseur-Action-Produit

        /// <summary>
        /// copie les demandes copiables de la livraison en cours d'un produit dans les aServir correspondants
        /// </summary>
        /// <param name="keyProduit"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/copieDemP")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieDemP([FromQuery] KeyUidRnoNo keyProduit)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKeyProduitOuLivraison(keyProduit);
            if (site == null)
            {
                return NotFound();
            }

            bool estFournisseur = await carte.EstActifEtAMêmeUidRno(site.KeyParam);
            if (!estFournisseur)
            {
                return Forbid();
            }

            RetourDeService retour = await _service.CopieDemandes(site, keyProduit);
            if (retour == null)
            {
                return RésultatBadRequest("RienACopier");
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// copie les demandes copiables de la livraison en cours dans les aServir correspondants
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        [HttpPost("/api/commande/copieDems")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieDems([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                return Forbid();
            }

            bool estFournisseur = await carte.EstActifEtAMêmeUidRno(keySite.KeyParam);
            if (!estFournisseur)
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            RetourDeService retour = await _service.CopieDemandes(site);
            if (retour == null)
            {
                return RésultatBadRequest("RienACopier");
            }

            return SaveChangesActionResult(retour);
        }

        #endregion
    }

}
