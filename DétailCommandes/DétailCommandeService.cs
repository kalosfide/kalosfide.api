using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Produits;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.DétailCommandes
{
    public class DétailCommandeService : KeyUidRnoNo2Service<DétailCommande, DétailCommandeVue>, IDétailCommandeService
    {
        public DétailCommandeService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.DétailCommande;
        }

        #region Utiles

        /// <summary>
        /// retourne le détail référencé, les champs Commande, Commande.Livraison et Livraison.Facture sont inclus
        /// </summary>
        /// <param name="aKeyDétail">KeyUidRnoNo2 d'un détail ou DétailCommande ou DétailCommandeVue</param>
        /// <returns></returns>
        public async Task<DétailCommande> Détail(AKeyUidRnoNo2 aKeyDétail)
        {
            DétailCommande détail = await _context.DétailCommande
                .Where(d => d.AMêmeKey(aKeyDétail))
                .Include(d => d.Commande)
                .ThenInclude(c => c.Livraison)
                .ThenInclude(l => l.Facture)
                .FirstOrDefaultAsync();
            return détail;
        }

        /// <summary>
        /// retourne la Commande du détail référencé
        /// </summary>
        /// <param name="aKeyDétail">KeyUidRnoNo2 d'un détail ou DétailCommande ou DétailCommandeVue</param>
        /// <returns></returns>
        public async Task<Commande> Commande(AKeyUidRnoNo2 aKeyDétail)
        {
            Commande commande = await _context.Commande
                .Where(c => c.Uid == aKeyDétail.Uid && c.Rno == aKeyDétail.Rno && c.No == aKeyDétail.No)
                .FirstOrDefaultAsync();
            return commande;
        }

        public decimal CoûtDemande(IEnumerable<DétailCommandeData> détails, Catalogue catalogue, out bool incomplet)
        {
            decimal coûtTotal = 0;
            bool complet = true;
            détails.ToList().ForEach(détail =>
            {
                ProduitData produit = catalogue.Produits.Where(p => p.No == détail.No).First();
                bool calculable = détail.TypeCommande == null || détail.TypeCommande == UnitéDeMesure.UnitéDeCommandeParDéfaut(produit.TypeMesure);
                if (détail.Demande.HasValue && calculable)
                {
                    coûtTotal += produit.Prix.Value * détail.Demande.Value;
                }
                else
                {
                    complet = false;
                }
            });
            incomplet = !complet;
            return coûtTotal;
        }

        public decimal CoûtALivrer(IEnumerable<DétailCommandeData> détails, Catalogue catalogue)
        {
            decimal coûtTotal = 0;
            détails.ToList().ForEach(détail =>
            {
                ProduitData produit = catalogue.Produits.Where(p => p.No == détail.No).First();
                    coûtTotal += produit.Prix.Value * détail.ALivrer.Value;
            });
            return coûtTotal;
        }

        public decimal CoûtAFacturer(IEnumerable<DétailCommandeData> détails, Catalogue catalogue)
        {
            decimal coûtTotal = 0;
            détails.ToList().ForEach(détail =>
            {
                ProduitData produit = catalogue.Produits.Where(p => p.No == détail.No).First();
                coûtTotal += produit.Prix.Value * détail.AFacturer.Value;
            });
            return coûtTotal;
        }

        public DétailCommandeData DétailCommandeData(DétailCommande détail)
        {
            DétailCommandeData data = new DétailCommandeData
            {
                No = détail.No2,
                TypeCommande = détail.TypeCommande,
                Demande = détail.Demande,
                ALivrer = détail.ALivrer,
                AFacturer = détail.AFacturer
            };
            return data;
        }

        #endregion

        public override DétailCommande CréeDonnée()
        {
            return new DétailCommande();
        }

        public override void CopieVueDansDonnée(DétailCommande donnée, DétailCommandeVue vue)
        {
            if (vue.Demande.HasValue)
            {
                donnée.TypeCommande = vue.TypeCommande;
                donnée.Demande = vue.Demande.Value;
            }
            donnée.ALivrer = vue.ALivrer;
            donnée.AFacturer = vue.AFacturer;
        }

        public override void CopieVuePartielleDansDonnée(DétailCommande donnée, DétailCommandeVue vuePartielle, DétailCommande donnéeEnregistrée)
        {
            donnée.TypeCommande = vuePartielle.TypeCommande ?? donnéeEnregistrée.TypeCommande;
            donnée.Demande = vuePartielle.Demande ?? donnéeEnregistrée.Demande;
            donnée.ALivrer = vuePartielle.ALivrer ?? donnéeEnregistrée.ALivrer;
            donnée.AFacturer = vuePartielle.AFacturer ?? donnéeEnregistrée.AFacturer;
        }

        public override DétailCommandeVue CréeVue(DétailCommande donnée)
        {
            DétailCommandeVue vue = new DétailCommandeVue
            {
                TypeCommande = donnée.TypeCommande,
                Demande = donnée.Demande,
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }

        /// <summary>
        /// crée un nouveau détail copie de l'ancien incrémente No de la commande
        /// </summary>
        /// <param name="ancien"></param>
        /// <returns></returns>
        private DétailCommande CréeCopieDétail(DétailCommande ancien)
        {
            DétailCommande nouveau = new DétailCommande
            {
                TypeCommande = ancien.TypeCommande,
                Demande = ancien.Demande
            };
            nouveau.CopieKey(ancien.KeyParam);
            nouveau.No++;
            return nouveau;
        }

        public async Task<RetourDeService<DétailCommande>> Ajoute(DétailCommandeVue vue)
        {
            DétailCommande détail = CréeDonnée();
            détail.CopieKey(vue.KeyParam);
            CopieVueDansDonnée(détail, vue);
            _context.DétailCommande.Add(détail);
            return await SaveChangesAsync(détail);
        }

        /// <summary>
        /// Crée des copies des détails dont le produit est toujours disponible avec un No (de la commande) augmenté de 1 et les ajoute sans sauver à la BDD
        /// </summary>
        /// <param name="commande"></param>
        public async Task<RetourDeService> AjouteCopiesDétails(Commande commande)
        {
            List<DétailCommande> détails = await _context.DétailCommande
                    .Where(d => d.Uid == commande.Uid && d.Rno == commande.Rno && d.No == commande.No)
                    .Include(d => d.Produit)
                    .Where(d => d.Produit.Etat == TypeEtatProduit.Disponible)
                    .ToListAsync();

            List<DétailCommande> copies = détails.Select(d => CréeCopieDétail(d)).ToList();
            _context.DétailCommande.AddRange(copies);
            return await SaveChangesAsync();
        }
    }
}
