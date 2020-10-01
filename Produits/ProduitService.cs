using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    class GèreArchive : GéreArchiveUidRnoNo<Produit, ProduitVue, ArchiveProduit>
    {
        public GèreArchive(DbSet<Produit> dbSet, DbSet<ArchiveProduit> dbSetArchive) : base(dbSet, dbSetArchive)
        {
        }

        protected override ArchiveProduit CréeArchive()
        {
            return new ArchiveProduit();
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
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveProduit);
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
            ErreurDeModel.AjouteAModelState(modelState, erreur, "prix");
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
                        ErreurDeModel.AjouteAModelState(modelState, "nomPris", "nom");
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
                        ErreurDeModel.AjouteAModelState(modelState, "nomPris", "nom");
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

        public async Task<string> NomCatégorie(Produit donnée)
        {
            return await _context.Catégorie
                .Where(catégorie => catégorie.Uid == donnée.Uid && catégorie.Rno == donnée.Rno && catégorie.No == donnée.CategorieNo)
                .Select(catégorie => catégorie.Nom)
                .FirstAsync();
        }

        public override void CopieVueDansDonnée(Produit donnée, ProduitVue vue)
        {
            if (vue.Nom != null)
            {
                donnée.Nom = vue.Nom;
            }
            if (vue.CategorieNo != null)
            {
                donnée.CategorieNo = vue.CategorieNo ?? 0;
            }
            if (vue.TypeCommande != null)
            {
                donnée.TypeCommande = vue.TypeCommande;
            }
            if (vue.TypeMesure != null)
            {
                donnée.TypeMesure = vue.TypeMesure;
            }
            if (vue.Prix != null)
            {
                donnée.Prix = vue.Prix ?? 0;
            }
            if (vue.Etat != null)
            {
                donnée.Etat = vue.Etat;
            }
        }

        public override void CopieVuePartielleDansDonnée(Produit donnée, ProduitVue vue, Produit donnéePourComplèter)
        {
            donnée.Nom = vue.Nom ?? donnéePourComplèter.Nom;
            donnée.CategorieNo = vue.CategorieNo ?? donnéePourComplèter.CategorieNo;
            donnée.TypeCommande = vue.TypeCommande ?? donnéePourComplèter.TypeCommande;
            donnée.TypeMesure = vue.TypeMesure ?? donnéePourComplèter.TypeMesure;
            donnée.Prix = vue.Prix ?? donnéePourComplèter.Prix;
            donnée.Etat = vue.Etat ?? donnéePourComplèter.Etat;
        }

        public override Produit CréeDonnée()
        {
            return new Produit();
        }

        private static void RemplitProduitDataDisponible(Produit donnée, IProduitData data)
        {
            data.Nom = donnée.Nom;
            data.CategorieNo = donnée.CategorieNo;
            data.TypeCommande = donnée.TypeCommande;
            data.TypeMesure = donnée.TypeMesure;
            data.Prix = donnée.Prix;
        }

        private static void RemplitProduitData(Produit donnée, IProduitData data)
        {
            RemplitProduitDataDisponible(donnée, data);
            data.Etat = donnée.Etat;
        }

        public ProduitData CréeProduitDataSansEtat(ArchiveProduit archive)
        {
            ProduitData produitData = new ProduitData
            {
                No = archive.No,
                CategorieNo = archive.CategorieNo.Value,
                Nom = archive.Nom,
                TypeCommande = archive.TypeCommande,
                TypeMesure = archive.TypeMesure,
                Prix = archive.Prix.Value,
            };
            return produitData;
        }

        public ProduitData CréeProduitDataAvecDate(ArchiveProduit archive)
        {
            ProduitData produitData = CréeProduitDataSansEtat(archive);
            produitData.Date = archive.Date;
            return produitData;
        }

        public override ProduitVue CréeVue(Produit donnée)
        {
            ProduitVue vue = new ProduitVue();
            vue.CopieKey(donnée.KeyParam);
            RemplitProduitData(donnée, vue);
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

        private static ProduitData CréeProduitDataDisponible(Produit donnée)
        {
            ProduitData data = new ProduitData
            {
                No = donnée.No
            };
            RemplitProduitDataDisponible(donnée, data);
            return data;
        }

        private static ProduitData CréeProduitData(Produit produit)
        {
            ProduitBilan bilan(string type)
            {
                var lignes = produit.Lignes.Where(l => l.Type == type);
                return new ProduitBilan
                {
                    Type = type,
                    Nb = lignes.Count(),
                    Quantité = lignes.Where(l => l.Quantité.HasValue).Select(l => l.Quantité.Value).Sum()
                };
            }
            ProduitData data = new ProduitData
            {
                No = produit.No,
                Bilans = new List<ProduitBilan>
                {
                    bilan("C"),
                    bilan("L"),
                    bilan("F")
                }
            };
            RemplitProduitData(produit, data);
            return data;
        }

        public async Task<List<ProduitData>> ProduitDatas(AKeyUidRno aKeySite)
        {
            List<ProduitData> produitDatas = await _context.Produit
                .Where(p => p.Uid == aKeySite.Uid && p.Rno == aKeySite.Rno)
                .Include(p => p.Lignes)
                .Select(p => CréeProduitData(p))
                .ToListAsync();
            return produitDatas;
        }

        public async Task<List<ProduitData>> ProduitDatasDisponibles(AKeyUidRno aKeySite)
        {
            return await _context.Produit
                .Where(p => p.Uid == aKeySite.Uid && p.Rno == aKeySite.Rno && p.Etat == TypeEtatProduit.Disponible)
                .Select(p => CréeProduitDataDisponible(p))
                .ToListAsync();
        }

        public async Task<bool> AChangé(KeyParam param)
        {
            return await _context.ArchiveProduit
                .Where(ep => ep.Uid == param.Uid && ep.Rno == param.Rno && DateTime.Compare(ep.Date, param.Date.Value) > 0)
                .AnyAsync();
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
