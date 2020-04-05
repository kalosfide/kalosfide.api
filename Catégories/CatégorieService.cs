using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Produits;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catégories
{
    class GèreArchive : Partages.KeyParams.GéreArchive<Catégorie, CatégorieVue, ArchiveCatégorie>
    {
        public GèreArchive(DbSet<Catégorie> dbSet, DbSet<ArchiveCatégorie> dbSetArchive) : base(dbSet, dbSetArchive)
        {
        }

        protected override ArchiveCatégorie CréeArchive()
        {
            return new ArchiveCatégorie();
        }

        protected override void CopieDonnéeDansArchive(Catégorie donnée, ArchiveCatégorie archive)
        {
            archive.Nom = donnée.Nom;
        }

        protected override ArchiveCatégorie CréeArchiveDesDifférences(Catégorie donnée, CatégorieVue vue)
        {
            bool modifié = false;
            ArchiveCatégorie archive = new ArchiveCatégorie();
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                archive.Nom = donnée.Nom;
                donnée.Nom = vue.Nom;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }
    public class CatégorieService : KeyUidRnoNoService<Catégorie, CatégorieVue>, ICatégorieService
    {
        public CatégorieService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Catégorie;
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveCatégorie);
            _inclutRelations = Complète;
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
            dValideSupprime = ValideSupprime;
        }

        public async Task<bool> NomPris(string nom)
        {
            return await _dbSet.Where(Catégorie => Catégorie.Nom == nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(AKeyUidRnoNo key, string nom)
        {
            return await _dbSet.Where(Catégorie => Catégorie.Nom == nom && (Catégorie.Uid != key.Uid || Catégorie.Rno != key.Rno || Catégorie.No != key.No)).AnyAsync();
        }

        private async Task ValideAjoute(Catégorie donnée, ModelStateDictionary modelState)
        {
            if (await NomPris(donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris", "nom");
            }
        }

        private async Task ValideEdite(Catégorie donnée, ModelStateDictionary modelState)
        {
            if (await NomPrisParAutre(donnée, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris", "nom");
            }
        }

        private async Task ValideSupprime(Catégorie donnée, ModelStateDictionary modelState)
        {
            bool avecProduits = await _context.Produit
                .Where(p => donnée.Uid == p.Uid && donnée.Rno == p.Rno && donnée.No == p.CategorieNo)
                .AnyAsync();
            if (avecProduits)
            {
                ErreurDeModel.AjouteAModelState(modelState, "nonVide");
            }
        }

        public override void CopieVueDansDonnée(Catégorie donnée, CatégorieVue vue)
        {
            if (vue.Nom != null)
            {
                donnée.Nom = vue.Nom;
            }
        }

        public override void CopieVuePartielleDansDonnée(Catégorie donnée, CatégorieVue vue, Catégorie donnéePourComplèter)
        {
            donnée.Nom = vue.Nom ?? donnéePourComplèter.Nom;
        }

        public override Catégorie CréeDonnée()
        {
            return new Catégorie();
        }

        IQueryable<Catégorie> Complète(IQueryable<Catégorie> données)
        {
            return données.Include(d => d.Produits);
        }

        public override CatégorieVue CréeVue(Catégorie donnée)
        {
            CatégorieVue vue = new CatégorieVue
            {
                Nom = donnée.Nom,
                NbProduits = donnée.Produits.Count
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }

        public CatégorieData CréeCatégorieData(ArchiveCatégorie archive)
        {
            CatégorieData catégorieData = new CatégorieData
            {
                No = archive.No,
                Nom = archive.Nom
            };
            return catégorieData;
        }

        public CatégorieData CréeCatégorieDataAvecDate(ArchiveCatégorie archive)
        {
            CatégorieData catégorieData = new CatégorieData
            {
                No = archive.No,
                Nom = archive.Nom,
                Date = archive.Date
            };
            return catégorieData;
        }

        public async Task<List<CatégorieData>> CatégorieDatas(AKeyUidRno aKeySite, List<ProduitData> produits)
        {
            IQueryable<Catégorie> queryCatégories = _context.Catégorie
                .Where(c => aKeySite.CommenceKey(c.KeyParam));
            if (produits != null)
            {
                queryCatégories = queryCatégories
                .Where(c => produits.Where(p => p.CategorieNo == c.No).Any());
            }
            List<CatégorieData> catégories = await queryCatégories
                .Select(c => new CatégorieData { No = c.No, Nom = c.Nom })
                .ToListAsync();
            return catégories;
        }

    }
}
