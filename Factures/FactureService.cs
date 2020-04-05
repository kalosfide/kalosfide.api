using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using KalosfideAPI.Utiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Factures
{
    class LCD
    {
        public Livraison Livraison { get; set; }
        public Commande Commande { get; set; }
        public IEnumerable<DétailCommande> Détails { get; set; }
    }

    public class FactureService : BaseService, IFactureService
    {
        private readonly IUtileService _utile;

        public FactureService(ApplicationContext _context, IUtileService utile) : base(_context)
        {
            _utile = utile;
        }

        private async Task<long> ProchainFactureNo(Site site)
        {
            Facture dernièreFacture = await _context.Facture
                .Where(f => site.Uid == f.SiteUid && site.Rno == f.SiteRno)
                .LastOrDefaultAsync();
            return dernièreFacture == null ? 1 : dernièreFacture.No + 1;
        }
        /// <summary>
        /// Retourne les ClientAFacturer de tous les clients ayant des commandes livrées non facturées
        /// contenant les CommandeAFacturer sans Détails
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<AFacturer> AFacturer(Site site)
        {
            IQueryable<Livraison> q_livraisonsTerminées = _context.Livraison.Where(l => l.Uid == site.Uid && l.Rno == site.Rno && l.Date.HasValue);
            IQueryable<Commande> q_commandesAvecNoLivraisonNonFacturées = _utile.CommandesAvecDétailsLivraisonEtFacture(
                filtreCommande: null, filtreDétails: null, filtreRole: null, keySite:site)
                .Where(c => c.Livraison != null && c.Livraison.Date.HasValue && !c.Livraison.FactureNo.HasValue);
            var q_liv_comAFacturer = q_livraisonsTerminées.Join(q_commandesAvecNoLivraisonNonFacturées,
                l => l.No, c => c.LivraisonNo, (l, c) => new { livraison = l, commande = c });

            List<LivraisonAFacturer> livraisonsAFacturer = await q_liv_comAFacturer
                .Select(lc => lc.livraison)
                .GroupBy(lc => lc.No, (no, livraisons) => livraisons.First())
                .Select(l => new LivraisonAFacturer { No = l.No, Date = l.Date.Value })
                .ToListAsync();

            var commandesAFacturerGroupéesParKey = await q_liv_comAFacturer
                .Select(lc => lc.commande)
                .GroupBy(c => new { c.Uid, c.Rno }, (key, kcs) => new { key.Uid, key.Rno, commandes = kcs })
                .ToListAsync();

            List<AFacturerDUnClient> aFacturerParClient = commandesAFacturerGroupéesParKey
                .Select(kcaf => AFacturerDUnClient(kcaf.Uid, kcaf.Rno, kcaf.commandes))
                .Where(claf => claf.Commandes.Count > 0)
                .ToList();

            long noProchaineFacture = await ProchainFactureNo(site);

            return new AFacturer { AFacturerParClient = aFacturerParClient, Livraisons = livraisonsAFacturer, NoProchaineFacture = noProchaineFacture };
        }

        private CommandeAFacturer CommandeAFacturer(Commande commande)
        {
            return new CommandeAFacturer
            {
                No = commande.No,
                LivraisonNo = commande.LivraisonNo.Value,
                Details = commande.Détails
                    .Where(d => d.ALivrer.HasValue && d.ALivrer.Value > 0)
                    .Select(d => new DétailAFacturer
                    {
                        No = d.No2,
                        ALivrer = d.ALivrer.Value,
                        AFacturer = d.AFacturer
                    }).ToList()
            };
        }

        private AFacturerDUnClient AFacturerDUnClient(string uid, int rno, IEnumerable<Commande> commandes)
        {
            var commandesAFacturer = commandes
                .Select(c => CommandeAFacturer(c))
                .Where(caf => caf.Details.Count > 0)
                .ToList();
            return new AFacturerDUnClient
            {
                Uid = uid,
                Rno = rno,
                Commandes = commandesAFacturer
            };
        }

        /// <summary>
        /// retourne la liste des CommandeAFacturer avec Détails d'un client
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        public async Task<List<CommandeAFacturer>> CommandesAFacturer(Site site, AKeyUidRno keyClient)
        {
            var x = await _utile.CommandesAvecDétailsLivraisonEtFacture(
                filtreCommande: c => keyClient.Uid == c.Uid && keyClient.Rno == c.Rno, filtreDétails: null, filtreRole: null, keySite: null)
                .Where(c => c.Livraison != null && c.Livraison.Date.HasValue && !c.Livraison.FactureNo.HasValue)

                .Join(_context.Livraison.Where(l => l.Uid == site.Uid && l.Rno == site.Rno),
                    c => c.LivraisonNo.Value, l => l.No, (c, l) => new { livraison = l, commande = c })
                .Where(lc => lc.livraison.Date.HasValue)
                .Select(lc => lc.commande)
                .GroupJoin(_context.DétailCommande,
                    c => new { c.Uid, c.Rno, c.No },
                    d => new { d.Uid, d.Rno, d.No },
                    (commande, détails) => new { commande, détails }
                   )
                .Where(cd => cd.détails.Where(d => d.ALivrer > 0).Any())
                .Select(cd => new CommandeAFacturer
                {
                    No = cd.commande.No,
                    LivraisonNo = cd.commande.LivraisonNo.Value,
                    Details = cd.détails.Select(d => new DétailAFacturer
                    {
                        No = d.No2,
                        ALivrer = d.ALivrer.Value,
                        AFacturer = d.AFacturer
                    }).ToList()
                })
                .ToListAsync();

            return x;
        }

        #region Ecriture

        public async Task<Commande> CommandeDeDétail(AKeyUidRnoNo2 aKeyDétail)
        {
            Commande commande = await _context.Commande
                .Where(c => c.CommenceKey(aKeyDétail.KeyParam))
                .FirstOrDefaultAsync();
            return commande;
        }

        public async Task<bool> EstDansSynthèseEnvoyée(Commande commande)
        {
            if (!commande.LivraisonNo.HasValue)
            {
                return false;
            }
            Livraison livraison = await _context.Livraison
                .Where(l => l.Uid == commande.SiteUid && l.Rno == commande.SiteRno && l.No == commande.LivraisonNo)
                .FirstOrDefaultAsync();
            if (!livraison.Date.HasValue)
            {
                return false;
            }
            return !commande.Livraison.FactureNo.HasValue;
        }

        public async Task<DétailCommande> LitDétail(DétailCommandeVue vue)
        {
            DétailCommande détail = await _context.DétailCommande
                .Where(d => d.AMêmeKey(vue))
                .FirstOrDefaultAsync();
            return détail;
        }
        public async Task<RetourDeService> EcritDétail(DétailCommande détail, DétailCommandeVue vue)
        {
            détail.AFacturer = vue.AFacturer;
            _context.DétailCommande.Update(détail);
            return await SaveChangesAsync();
        }

        public async Task<Commande> LitCommande(AKeyUidRnoNo keyCommande)
        {
            Commande commande = await _context.Commande
                .Where(d => d.AMêmeKey(keyCommande))
                .FirstOrDefaultAsync();
            return commande;
        }
        public void CopieCommandeSansSauver(Commande commande)
        {
            commande.Détails.ToList().ForEach(détail => détail.AFacturer = détail.ALivrer);
            _context.DétailCommande.UpdateRange(commande.Détails);
        }
        public void AnnuleCommandeSansSauver(Commande commande)
        {
            commande.Détails.ToList().ForEach(détail => détail.AFacturer = 0);
            _context.DétailCommande.UpdateRange(commande.Détails);
        }
        public async Task<RetourDeService> CopieCommande(Commande commande)
        {
            CopieCommandeSansSauver(commande);
            return await SaveChangesAsync();
        }
        public async Task<RetourDeService> AnnuleCommande(Commande commande)
        {
            AnnuleCommandeSansSauver(commande);
            return await SaveChangesAsync();
        }

        public async Task<List<Commande>> CommandesLivréesEtNonFacturées(Site site, AKeyUidRno keyClient)
        {
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(
                filtreCommande: c => keyClient.Uid == c.Uid && keyClient.Rno == c.Rno, filtreDétails: null, filtreRole: null, keySite: null)
                .Where(c => c.Livraison != null && c.Livraison.Date.HasValue && !c.Livraison.FactureNo.HasValue)
                .ToListAsync();

            return commandes;
        }
        public async Task<RetourDeService> CopieCommandes(List<Commande> commandes)
        {
            commandes.ForEach(commande => CopieCommandeSansSauver(commande));
            return await SaveChangesAsync();
        }
        public async Task<RetourDeService> AnnuleCommandes(List<Commande> commandes)
        {
            commandes.ForEach(commande => AnnuleCommandeSansSauver(commande));
            return await SaveChangesAsync();
        }
        public async Task<RetourDeService> FactureCommandes(Site site, AKeyUidRno keyClient, List<Commande> commandes)
        {
            long factureNo = await ProchainFactureNo(site);
            Facture facture = new Facture
            {
                Uid = keyClient.Uid,
                Rno = keyClient.Rno,
                No = factureNo,
                SiteUid = site.Uid,
                SiteRno = site.Rno,
                Date = DateTime.Now
            };
            _context.Facture.Add(facture);
            commandes.ForEach(commande => commande.Livraison.FactureNo = factureNo);
            _context.Commande.UpdateRange(commandes);
            return await SaveChangesAsync();
        }

        #endregion
    }
}