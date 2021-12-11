using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    class GèreArchive : GéreArchiveUidRnoNo<Produit, ProduitVue, ArchiveProduit>
    {
        public GèreArchive(
            DbSet<Produit> dbSet,
            IIncludableQueryable<Produit, ICollection<ArchiveProduit>> query,
            Func<Produit, ICollection<ArchiveProduit>> archives,
            DbSet<ArchiveProduit> dbSetArchive,
            IIncludableQueryable<ArchiveProduit, Produit> queryArchive,
            Func<ArchiveProduit, Produit> donnée
            ) : base(dbSet, query, archives, dbSetArchive, queryArchive, donnée)
        {
        }

        protected override ArchiveProduit CréeArchive()
        {
            return new ArchiveProduit();
        }

        protected override void CopieArchiveDansArchive(ArchiveProduit de, ArchiveProduit vers)
        {
            if (de.CategorieNo != null) { vers.CategorieNo = de.CategorieNo.Value; }
            if (de.Nom != null) { vers.Nom = de.Nom; }
            if (de.TypeCommande != null) { vers.TypeCommande = de.TypeCommande; }
            if (de.TypeMesure != null) { vers.TypeMesure = de.TypeMesure; }
            if (de.Prix != null) { vers.Prix = de.Prix.Value; }
            if (de.Etat != null) { vers.Etat = de.Etat; }
        }

        protected override void CopieDonnéeDansArchive(Produit produit, ArchiveProduit archive)
        {
            archive.CategorieNo = produit.CategorieNo;
            archive.Nom = produit.Nom;
            archive.TypeCommande = produit.TypeCommande;
            archive.TypeMesure = produit.TypeMesure;
            archive.Prix = produit.Prix;
            archive.Etat = produit.Etat;
        }

        protected override ArchiveProduit CréeArchiveDesDifférences(Produit donnée, ProduitVue vue)
        {
            bool modifié = false;
            ArchiveProduit archive = new ArchiveProduit
            {
                Date = DateTime.Now
            };
            if (vue.CategorieNo != null && donnée.CategorieNo != vue.CategorieNo)
            {
                donnée.CategorieNo = vue.CategorieNo.Value;
                archive.CategorieNo = vue.CategorieNo;
                modifié = true;
            }
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                donnée.Nom = vue.Nom;
                archive.Nom = vue.Nom;
                modifié = true;
            }
            if (vue.TypeCommande != null && donnée.TypeCommande != vue.TypeCommande)
            {
                donnée.TypeCommande = vue.TypeCommande;
                archive.TypeCommande = vue.TypeCommande;
                modifié = true;
            }
            if (vue.TypeMesure != null && donnée.TypeMesure != vue.TypeMesure)
            {
                donnée.TypeMesure = vue.TypeMesure;
                archive.TypeMesure = vue.TypeMesure;
                modifié = true;
            }
            if (vue.Prix != null && donnée.Prix != vue.Prix)
            {
                donnée.Prix = vue.Prix.Value;
                archive.Prix = vue.Prix;
                modifié = true;
            }
            if (vue.Etat != null && donnée.Etat != vue.Etat)
            {
                donnée.Etat = vue.Etat;
                archive.Etat = vue.Etat;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class ProduitService : KeyUidRnoNoService<Produit, ProduitVue>, IProduitService
    {
        public ProduitService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Produit;
            _géreArchive = new GèreArchive(
                _dbSet, _dbSet.Include(produit => produit.Archives), (Produit produit) => produit.Archives,
                _context.ArchiveProduit, _context.ArchiveProduit.Include(a => a.Produit), (ArchiveProduit archive) => archive.Produit
                );
            _inclutRelations = Complète;
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
            dValideSupprime = ValideSupprime;
        }

        private bool VérifieTypeMesure(Produit donnée, ModelStateDictionary modelState)
        {
            if (!UnitéDeMesure.EstValide(donnée.TypeMesure))
            {
                ErreurDeModel.AjouteAModelState(modelState, "typeMesure");
                return false;
            }
            return true;
        }

        private bool VérifieTypeCommande(Produit donnée, ModelStateDictionary modelState)
        {
            if (!TypeUnitéDeCommande.EstValide(donnée.TypeCommande, donnée.TypeMesure))
            {
                ErreurDeModel.AjouteAModelState(modelState, "typeCommande");
                return false;
            }
            return true;
        }

        private bool VérifieTypeUnités(Produit donnée, ModelStateDictionary modelState)
        {
            if (!VérifieTypeMesure(donnée, modelState))
            {
                return false;
            }
            return VérifieTypeCommande(donnée, modelState);
        }

        private bool VérifiePrix(Produit donnée, ModelStateDictionary modelState)
        {
            string erreur = PrixProduitDef.Vérifie(donnée.Prix);
            if (erreur == null)
            {
                return true;
            }
            ErreurDeModel.AjouteAModelState(modelState, "prix", erreur);
            return false;
        }

        private async Task ValideAjoute(Produit donnée, ModelStateDictionary modelState)
        {
            if (VérifieTypeUnités(donnée, modelState))
            {
                if (VérifiePrix(donnée, modelState))
                {
                    if (await NomPris(donnée.Uid, donnée.Rno, donnée.Nom))
                    {
                        ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
                    }
                }
            }
        }

        private async Task ValideEdite(Produit donnée, ModelStateDictionary modelState)
        {
            if (VérifieTypeUnités(donnée, modelState))
            {
                if (VérifiePrix(donnée, modelState))
                {
                    if (await NomPrisParAutre(donnée.Uid, donnée.Rno, donnée.No, donnée.Nom))
                    {
                        ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
                    }
                }
            }
        }

        private async Task ValideSupprime(Produit donnée, ModelStateDictionary modelState)
        {
            KeyUidRno keySite = new KeyUidRno
            {
                Uid = donnée.Uid,
                Rno = donnée.Rno
            };
            bool avecCommandes = await _context.Lignes
                .Where(l => l.Uid2 == donnée.Uid && l.Rno2 == donnée.Rno && l.No2 == donnée.No)
                .AnyAsync();
            if (avecCommandes)
            {
                ErreurDeModel.AjouteAModelState(modelState, "supprime");
            }
        }

        IQueryable<Produit> Complète(IQueryable<Produit> produits)
        {
            return produits.Include(p => p.Catégorie);
        }

        protected override void CopieVueDansDonnée(ProduitVue de, Produit vers)
        {
            if (de.Nom != null)
            {
                vers.Nom = de.Nom;
            }
            if (de.CategorieNo != null)
            {
                vers.CategorieNo = de.CategorieNo ?? 0;
            }
            if (de.TypeCommande != null)
            {
                vers.TypeCommande = de.TypeCommande;
            }
            if (de.TypeMesure != null)
            {
                vers.TypeMesure = de.TypeMesure;
            }
            if (de.Prix != null)
            {
                vers.Prix = de.Prix ?? 0;
            }
            if (de.Etat != null)
            {
                vers.Etat = de.Etat;
            }
        }

        protected override void CopieVuePartielleDansDonnée(ProduitVue de, Produit vers, Produit pourComplèter)
        {
            vers.Nom = de.Nom ?? pourComplèter.Nom;
            vers.CategorieNo = de.CategorieNo ?? pourComplèter.CategorieNo;
            vers.TypeCommande = de.TypeCommande ?? pourComplèter.TypeCommande;
            vers.TypeMesure = de.TypeMesure ?? pourComplèter.TypeMesure;
            vers.Prix = de.Prix ?? pourComplèter.Prix;
            vers.Etat = de.Etat ?? pourComplèter.Etat;
        }

        public override Produit CréeDonnée()
        {
            return new Produit
            {
                // la date sera mise à jour à la fin de la modification

                Date = DateTime.Now
            };
        }

        public override ProduitVue CréeVue(Produit donnée)
        {
            ProduitVue vue = new ProduitVue();
            vue.CopieKey(donnée);
            ProduitFabrique.Copie(donnée, vue);
            return vue;
        }

        public async Task<bool> NomPris(string siteUid, int siteRno, string nom)
        {
            return await _dbSet
                .Where(produit => produit.Uid == siteUid && produit.Rno == siteRno && produit.Nom == nom)
                .AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(string siteUid, int siteRno, long produitNo, string nom)
        {
            return await _dbSet
                .Where(produit => produit.Uid == siteUid && produit.Rno == siteRno && produit.Nom == nom && produit.No != produitNo)
                .AnyAsync();
        }

        public async Task<List<ProduitDeCatalogue>> ProduitsDeCatalogue(AKeyUidRno aKeySite)
        {
            List<Produit> produits = await _context.Produit
                .Where(p => p.Uid == aKeySite.Uid && p.Rno == aKeySite.Rno)
                .Include(p => p.Lignes)
                .ToListAsync();
            return produits.Select(p => ProduitDeCatalogue.AvecEtat(p)).ToList();
        }

        public async Task<List<ProduitDeCatalogue>> ProduitsDeCatalogueDisponibles(AKeyUidRno aKeySite)
        {
            List<Produit> produits = await _context.Produit
                .Where(p => p.Uid == aKeySite.Uid && p.Rno == aKeySite.Rno && p.Etat == TypeEtatProduit.Disponible)
                .Include(p => p.Lignes)
                .ToListAsync();
            return produits.Select(p => ProduitDeCatalogue.SansEtatNiDate(p)).ToList();
        }

        /// <summary>
        /// supprime toutes les lignes demandant un produit si la commande n'est pas envoyée
        /// appelé quand un produit cesse d'être Disponible
        /// </summary>
        /// <param name="akeyProduit"></param>
        /// <returns></returns>
        public async Task SupprimeDétailsCommandesSansLivraison(AKeyUidRnoNo akeyProduit)
        {
            List<LigneCLF> lignes = await _context.Docs
                .Where(d => d.Date.HasValue && d.Type == "C" && d.SiteUid == akeyProduit.Uid && d.SiteRno == akeyProduit.Rno)
                .Join(_context.Lignes,
                    c => new { c.Uid, c.Rno, c.No },
                    d => new { d.Uid, d.Rno, d.No },
                    (c, d) => d
                    )
                .Where(d => d.No2 == akeyProduit.No)
                .ToListAsync();
            _context.Lignes.RemoveRange(lignes);
            await _context.SaveChangesAsync();
        }

    }
}
