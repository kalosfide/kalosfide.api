using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using KalosfideAPI.Utiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{
    public class CommandeService : BaseService<Commande>, ICommandeService
    {
        private readonly IDétailCommandeService _détailService;
        private readonly IUtileService _utile;
        private readonly IClientService _clientService;

        public CommandeService(ApplicationContext context,
            IUtileService utile,
            IDétailCommandeService détailService,
            IClientService clientService
            ) : base(context)
        {
            _utile = utile;
            _détailService = détailService;
            _clientService = clientService;
        }

        #region Utiles

        /// <summary>
        /// retourne la commande définie par keyOuVueCommande
        /// </summary>
        /// <param name="keyOuVueCommande"></param>
        /// <returns></returns>
        public async Task<Commande> CommandeDeKeyOuVue(AKeyUidRnoNo keyOuVueCommande)
        {
            bool filtre(Commande c) => c.Uid == keyOuVueCommande.Uid && c.Rno == keyOuVueCommande.Rno && c.No == keyOuVueCommande.No;
            Commande commande = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtre , null, null, null)
                .FirstOrDefaultAsync();
            return commande;
        }

        /// <summary>
        /// retourne la dernière commande du client défini par keyClient
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        public async Task<Commande> DernièreCommande(AKeyUidRno keyClient)
        {
            bool filtre(Commande c) => c.Uid == keyClient.Uid && c.Rno == keyClient.Rno;
            Commande commande = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtre , null, null, null)
                .LastOrDefaultAsync();
            return commande;
        }

        /// <summary>
        /// retourne vrai si les TypeCommande du détail et du produit permettent la copie de Demande dans ALivrer
        /// </summary>
        /// <param name="détail"></param>
        /// <param name="produit"></param>
        /// <returns></returns>
        public bool TypesPermettentCopieDemande(DétailCommande détail, Produit produit)
        {
            return produit.TypeCommande != TypeUnitéDeCommande.UnitéOuVrac || détail.TypeCommande == TypeUnitéDeCommande.Vrac;
        }
        private bool EtatSitePermetModification(Commande commande, Site site, Livraison dernièreLivraison)
        {
            return commande.LivraisonNo.HasValue
                ? site.Etat == TypeEtatSite.Livraison && commande.LivraisonNo.Value == dernièreLivraison.No
                : site.Etat == TypeEtatSite.Ouvert;
        }

        /// <summary>
        /// retourne les dernières Commandes des clients actifs ou nouveau du site défini par keySite
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public async Task<List<Commande>> DernièresCommandes(AKeyUidRno keySite)
        {
            bool filtreCommande(Commande commande) => !commande.Date.HasValue || commande.Date.Value.Ticks == 0;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommande, null, _utile.FiltreRoleActif(), keySite).ToListAsync();
            return commandes;
        }

        /// <summary>
        /// retourne les commandes reçues depuis la date
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public async Task<List<Commande>> CommandesOuvertesDesClientsAvecCompte(AKeyUidRno keySite)
        {
            // ce sont celles qui n'ont pas de date
            bool filtreCommande(Commande commande) => !commande.Date.HasValue;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommande, null, null, keySite).ToListAsync();
            return commandes;
        }

        /// <summary>
        /// retourne la liste des commandes sans numéro de livraison émises par les clients autorisés d'un site
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public async Task<List<Commande>> CommandesOuvertes(AKeyUidRno keySite)
        {
            bool filtreCommande(Commande commande) => commande.Date.HasValue && !commande.LivraisonNo.HasValue;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommande, null, null, keySite).ToListAsync();
            return commandes;
        }

        /// <summary>
        /// fixe le numéro de livraison des commandes sans numéro de livraison émises par !es clients autorisés d'un site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraisonNo"></param>
        /// <returns></returns>
        public async Task CommenceLivraison(Site site, long livraisonNo)
        {
            List<Commande> commandes = await CommandesOuvertes(site);
            commandes.ForEach(c => c.LivraisonNo = livraisonNo);
            _context.Commande.UpdateRange(commandes);
        }

        /// <summary>
        /// supprime le numéro de livraison des commandes de la livraison
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraisonNo"></param>
        /// <returns></returns>
        public async Task AnnuleLivraison(Site site, long livraisonNo)
        {
            bool filtreCommande(Commande commande) => commande.LivraisonNo == livraisonNo;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommande, null, null, site).ToListAsync();
            commandes.ForEach(c => c.LivraisonNo = null);
            _context.Commande.UpdateRange(commandes);
        }

        /// <summary>
        /// fixe la date à celle du dernier détail créé par le client s'il y en a, à celle de la livraison sinon.
        /// </summary>
        /// <param name="commande"></param>
        /// <param name="dateLivraison"></param>
        /// <returns></returns>

        private Commande TermineCommande(Commande commande, DateTime dateLivraison)
        {
            if (commande.Date.Value.Ticks == 0)
            {
                commande.Date = dateLivraison;
            }
            return commande;
        }

        /// <summary>
        /// supprime les commandes vides, fixe la date des autres à celle de leur dernier détail créé par le client s'il y en a, à celle de la livraison sinon.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraison"></param>
        /// <returns></returns>
        public async Task TermineLivraison(Site site, Livraison livraison)
        {
            bool filtreCommande(Commande commande) => commande.LivraisonNo == livraison.No;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommande, null, null, site).ToListAsync();
            List<Commande> vides = commandes.Where(c => c.Détails.Count == 0).ToList();
            commandes = commandes.Where(c => c.Détails.Count != 0).ToList();
            commandes.ForEach(commande => TermineCommande(commande, livraison.Date.Value));
            _context.Commande.RemoveRange(vides);
            _context.Commande.UpdateRange(commandes);
        }

        public CommandeVue CréeCommandeVue(Commande commande)
        {
            CommandeVue vue = new CommandeVue
            {
                Date = commande.Date,
                LivraisonNo = commande.LivraisonNo,
                Details = (new List<DétailCommande>(commande.Détails))
                    .Select(détail => new DétailCommandeData
                    {
                        No = détail.No2,
                        TypeCommande = détail.TypeCommande,
                        Demande = détail.Demande,
                        ALivrer = détail.ALivrer
                    }).ToList()
            };
            vue.CopieKey(commande.KeyParam);
            return vue;
        }

        #endregion

        #region Création

        public async Task<RetourDeService<Commande>> AjouteCommande(AKeyUidRno keyClient, long noCommande, Site site, bool estFournisseur)
        {
            Commande commande = new Commande
            {
                Uid = keyClient.Uid,
                Rno = keyClient.Rno,
                No = noCommande,
                SiteUid = site.Uid,
                SiteRno = site.Rno,
            };
            if (estFournisseur)
            {
                commande.Date = new DateTime();
            }

            // [A SUPPRIMER
            if (site.Etat == TypeEtatSite.Livraison)
            {
                Livraison livraison = await _utile.DernièreLivraison(site);
                commande.LivraisonNo = livraison.No;
            }
            // A SUPPRIMER]

            _context.Commande.Add(commande);
            return await SaveChangesAsync(commande);
        }

        #endregion // Création

        #region Client-Lecture

        public async Task<ContexteCommande> ContexteCommande(Site site)
        {
            ContexteCommande contexte = new ContexteCommande
            {
                EtatSite = site.Etat,
                DateCatalogue = await _utile.DateCatalogue(site)
            };
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison != null)
            {
                contexte.NoLivraison = livraison.No;
                if (livraison.Date.HasValue)
                {
                    contexte.DateLivraison = livraison.Date.Value;
                }
            }
            else
            {
                contexte.NoLivraison = 0;
            }
            return contexte;
        }

        /// <summary>
        /// retourne un ContexteCommande contenant les données d'état définissant les droits
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <param name="site">Site du client</param>
        /// <returns>CommandesVue</returns>
        public async Task<ContexteCommande> Contexte(AKeyUidRno keyClient, Site site)
        {
            ContexteCommande contexte = await ContexteCommande(site);
            Commande dernièreCommande = await DernièreCommande(keyClient);
            if (dernièreCommande != null)
            {
                contexte.NoDC = dernièreCommande.No;
            }
            return contexte;
        }

        /// <summary>
        /// retourne un CommandeVue contenant la dernière Commande d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns>CommandesVue</returns>
        public async Task<CommandeVue> EnCours(AKeyUidRno keyClient)
        {
            Commande dernièreCommande = await DernièreCommande(keyClient);
            if (dernièreCommande != null)
            {
                return CréeCommandeVue(dernièreCommande);
            }
            return null;
        }

        #endregion

        #region Fournisseur-Action

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande du site dont la demande est copiable
        /// et dont la Commande passe le filtre des Commandes si présent et le Produit passe le filtre des produits si présent
        /// </summary>
        /// <param name="site"></param>
        /// <param name="filtreClient"></param>
        /// <param name="filtreProduit"></param>
        /// <param name="filtreDétail"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        private async Task<RetourDeService> CopieDemandes(Site site,
            Func<Commande, bool> filtreClient,
            Func<Produit, bool> filtreProduit,
            Func<DétailCommande, bool> filtreDétail)
        {
            IQueryable<DétailCommande> détails = _utile.DétailsAvecProduitCommandeLivraisonEtFacture(filtreDétail, filtreClient, null, site);
            if (filtreProduit != null)
            {
                détails = détails.Where(d => filtreProduit(d.Produit));
            }

            List<DétailCommande> copiables = await détails
                .Where(d => d.Commande.Date.HasValue && d.Commande.LivraisonNo.HasValue && !d.Commande.Livraison.Date.HasValue)
                .Where(d => d.Produit.TypeCommande != TypeUnitéDeCommande.UnitéOuVrac || d.TypeCommande == TypeUnitéDeCommande.Vrac)
                .ToListAsync();


            if (copiables.Count == 0)
            {
                return null;
            }
            copiables.ForEach(d => d.ALivrer = d.Demande);
            _context.DétailCommande.UpdateRange(copiables);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande du site dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        public async Task<RetourDeService> CopieDemandes(Site site)
        {
            return await CopieDemandes(site, filtreClient: null, filtreProduit: null, filtreDétail: null);
        }

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande du client dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        public async Task<RetourDeService> CopieDemandes(Site site, KeyUidRno keyClient)
        {
            return await CopieDemandes(site, filtreClient: c => keyClient.CommenceKey(c.KeyParam), filtreProduit: null, filtreDétail: null);
        }

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande demandant le produit dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyProduit"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        public async Task<RetourDeService> CopieDemandes(Site site, KeyUidRnoNo keyProduit)
        {
            return await CopieDemandes(site, filtreClient: null, filtreProduit: p => p.No == keyProduit.No, filtreDétail: null);
        }

        /// <summary>
        /// Copie Demande dans AServir pour le DétailCommande si la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyDétail"></param>
        /// <returns>null si le détail n'est pas copiable</returns>
        public async Task<RetourDeService> CopieDemandes(Site site, KeyUidRnoNo2 keyDétail)
        {
            return await CopieDemandes(site, filtreClient: null, filtreProduit: null, filtreDétail: d => d.AMêmeKey(keyDétail));
        }

        #endregion

        #region Fournisseur-Lecture

        /// <summary>
        /// retourne un CommandesVue contenant les dernières Commande non vides de tous les clients d'un site
        /// </summary>
        /// <param name="keySite">key du Site</param>
        /// <returns>CommandesVue</returns>
        public async Task<List<CommandeVue>> CommandesVue(AKeyUidRno keySite)
        {
            Role[] roles = await _context.Role
                .Where(r => r.Uid != r.SiteUid && r.SiteUid == keySite.Uid && r.SiteRno == keySite.Rno)
                .ToArrayAsync();
            var commandes = new List<CommandeVue>();
            for (int i = 0; i < roles.Count(); i++)
            {
                Role role = roles[i];
                Commande commande = await DernièreCommande(role);
                if (commande != null)
                {
                    commandes.Add(CréeCommandeVue(commande));
                }
            }
            return commandes;
        }

        #endregion

        /// <summary>
        /// Supprime la commande et ses détails si la commande a été créée par l'utilisateur.
        /// Refuse la commande en fixant les ALivrer des détails à 0, si l'utilisateur est le fournisseur et la commande a été créée par le client.
        /// </summary>
        /// <param name="commande"></param>
        /// <param name="parLeClient">vrai si l'action est faite par le client</param>
        /// <returns></returns>
        public async Task<RetourDeService> SupprimeOuRefuse(Commande commande, bool parLeClient)
        {
            if (parLeClient)
            {
                _context.DétailCommande.RemoveRange(commande.Détails);
                _context.Commande.Remove(commande);
            }
            else
            {
                if (commande.Date.HasValue && commande.Date.Value.Ticks > 0) // créée par le client
                {
                    (new List<DétailCommande>(commande.Détails)).ForEach(d => d.ALivrer = 0);
                    _context.DétailCommande.UpdateRange(commande.Détails);
                }
                else
                {
                    _context.DétailCommande.RemoveRange(commande.Détails);
                    _context.Commande.Remove(commande);
                }
            }

            return await SaveChangesAsync();
        }

    }
}
