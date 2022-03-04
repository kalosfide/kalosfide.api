using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
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
    class GèreArchive : AvecIdEtSiteIdGèreArchive<Catégorie, CatégorieAEditer, ArchiveCatégorie>
    {
        public GèreArchive(DbSet<ArchiveCatégorie> dbSetArchive) : base(dbSetArchive)
        {
        }

        protected override ArchiveCatégorie CréeArchive()
        {
            return new ArchiveCatégorie();
        }
        protected override void CopieDonnéeDansArchive(Catégorie donnée, ArchiveCatégorie archive)
        {
            Catégorie.CopieData(donnée, archive);
        }

        protected override ArchiveCatégorie CréeArchiveDesDifférences(Catégorie donnée, CatégorieAEditer vue)
        {
            ArchiveCatégorie archive = new ArchiveCatégorie
            {
                Date = DateTime.Now
            };
            bool modifié = Catégorie.CopieDifférences(donnée, vue, archive);
            return modifié ? archive : null;
        }
        protected override IQueryable<ArchiveCatégorie> ArchivesAvecDonnée(uint idSite)
        {
            return _dbSetArchive
                .Include(a => a.Catégorie)
                .Where(a => a.Catégorie.SiteId == idSite);
        }

        protected override Catégorie DonnéeDeArchive(ArchiveCatégorie archive)
        {
            return archive.Catégorie;
        }

        protected override void CopieArchiveDansArchive(ArchiveCatégorie de, ArchiveCatégorie vers)
        {
            Catégorie.CopieDataSiPasNull(de, vers);
        }
    }
    public class CatégorieService : AvecIdEtSiteIdService<Catégorie, CatégorieAAjouter, CatégorieAEnvoyer, CatégorieAEditer>, ICatégorieService
    {
        public CatégorieService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Catégorie;
            _gèreArchive = new GèreArchive(_context.ArchiveCatégorie);
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
            dValideSupprime = ValideSupprime;
        }

        public async Task<bool> NomPris(string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(uint id, string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom && c.Id != id).AnyAsync();
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
            if (await NomPrisParAutre(donnée.Id, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
        }

        private async Task ValideSupprime(Catégorie donnée, ModelStateDictionary modelState)
        {
            bool avecProduits = await _context.Produit
                .Where(p => donnée.Id == p.CategorieId)
                .AnyAsync();
            if (avecProduits)
            {
                ErreurDeModel.AjouteAModelState(modelState, "nonVide");
            }
        }

        protected override void CopieAjoutDansDonnée(CatégorieAAjouter de, Catégorie vers)
        {
            vers.SiteId = de.SiteId;
            Catégorie.CopieData(de, vers);
        }

        protected override void CopieEditeDansDonnée(CatégorieAEditer de, Catégorie vers)
        {
            Catégorie.CopieDataSiPasNull(de, vers);
        }

        protected override void CopieVuePartielleDansDonnée(CatégorieAEditer de, Catégorie vers, Catégorie pourCompléter)
        {
            Catégorie.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
        }

        public override Catégorie CréeDonnée()
        {
            return new Catégorie();
        }

        protected override CatégorieAEnvoyer Ajouté(Catégorie donnée, DateTime date)
        {
            return CatégorieAEnvoyer.SansDate(donnée);
        }

        public async Task<List<CatégorieAEnvoyer>> CatégoriesDeCatalogue(uint idSite)
        {
            List<Catégorie> catégories = await _context.Catégorie
                .Where(c => c.SiteId == idSite)
                .ToListAsync();
            return catégories.Select(c => CatégorieAEnvoyer.SansDate(c)).ToList();
        }

        public async Task<List<CatégorieAEnvoyer>> CatégoriesDeCatalogueDesDisponibles(uint idSite)
        {
            List<Catégorie> catégories = await _context.Catégorie
                .Where(c => c.SiteId == idSite)
                .Include(c => c.Produits)
                .Where(c => c.Produits.Where(p => p.Disponible).Any())
                .ToListAsync();
            return catégories.Select(c => CatégorieAEnvoyer.SansDate(c)).ToList();
        }

    }
}
