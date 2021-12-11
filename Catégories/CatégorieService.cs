using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Produits;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catégories
{
    class GèreArchive : GéreArchiveUidRnoNo<Catégorie, CatégorieVue, ArchiveCatégorie>
    {
        public GèreArchive(
            DbSet<Catégorie> dbSet,
            IIncludableQueryable<Catégorie, ICollection<ArchiveCatégorie>> query,
            Func<Catégorie, ICollection<ArchiveCatégorie>> archives,
            DbSet<ArchiveCatégorie> dbSetArchive,
            IIncludableQueryable<ArchiveCatégorie, Catégorie> queryArchive,
            Func<ArchiveCatégorie, Catégorie> donnée
            ) : base(dbSet, query, archives, dbSetArchive, queryArchive, donnée)
        {
        }

        protected override ArchiveCatégorie CréeArchive()
        {
            return new ArchiveCatégorie();
        }

        protected override void CopieArchiveDansArchive(ArchiveCatégorie de, ArchiveCatégorie vers)
        {
            if (de.Nom != null) { vers.Nom = de.Nom; }
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
            _géreArchive = new GèreArchive(
                _dbSet, _dbSet.Include(catégorie => catégorie.Archives), (Catégorie catégorie) => catégorie.Archives,
                _context.ArchiveCatégorie, _context.ArchiveCatégorie.Include(a => a.Catégorie), (ArchiveCatégorie archive) => archive.Catégorie
                );
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
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
        }

        private async Task ValideEdite(Catégorie donnée, ModelStateDictionary modelState)
        {
            if (await NomPrisParAutre(donnée, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
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

        protected override void CopieVueDansDonnée(CatégorieVue de, Catégorie vers)
        {
            if (de.Nom != null)
            {
                vers.Nom = de.Nom;
            }
        }

        protected override void CopieVuePartielleDansDonnée(CatégorieVue de, Catégorie vers, Catégorie pourComplèter)
        {
            vers.Nom = de.Nom ?? pourComplèter.Nom;
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
            return new CatégorieVue(donnée);
        }

        public async Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogue(AKeyUidRno aKeySite)
        {
            List<Catégorie> catégories = await _context.Catégorie
                .Where(c => aKeySite.Uid == c.Uid && aKeySite.Rno == c.Rno)
                .ToListAsync();
            return catégories.Select(c => CatégorieDeCatalogue.SansDate(c)).ToList();
        }

        public async Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogueDesDisponibles(AKeyUidRno aKeySite)
        {
            List<Catégorie> catégories = await _context.Catégorie
                .Where(c => aKeySite.Uid == c.Uid && aKeySite.Rno == c.Rno)
                .Include(c => c.Produits)
                .Where(c => c.Produits.Where(p => p.Etat==TypeEtatProduit.Disponible).Any())
                .ToListAsync();
            return catégories.Select(c => CatégorieDeCatalogue.SansDate(c)).ToList();
        }

    }
}
