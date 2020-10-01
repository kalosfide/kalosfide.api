using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using KalosfideAPI.Utiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{

    public class CatalogueService : ICatalogueService
    {
        private readonly ApplicationContext _context;
        private readonly IProduitService _produitService;
        private readonly ICatégorieService _catégorieService;

        public CatalogueService(ApplicationContext context,
            IProduitService produitService,
            ICatégorieService catégorieService)
        {
            _context = context;
            _produitService = produitService;
            _catégorieService = catégorieService;
        }

        /// <summary>
        /// retourne la dernière date de fin de modification du catalogue antérieure à la date si présente ou DateNulle si la modification est en cours
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<DateTime> DateCatalogue(AKeyUidRno keySite, DateTime? date)
        {
            IQueryable<ArchiveSite> archivesQuery = _context.ArchiveSite.Where(a => keySite.Uid == a.Uid && keySite.Rno == a.Rno)
                .OrderBy(a => a.Date);
            if (date != null)
            {
                archivesQuery = archivesQuery.Where(a => a.Date < date.Value);
            }
            // dernierDébutDEtatCatalogue existe toujours car le site est créé dans l'état Catalogue
            ArchiveSite dernierDébutDEtatCatalogue = await archivesQuery.Where(a => a.Etat == TypeEtatSite.Catalogue).LastAsync();
            // dernièreFinDEtatCatalogue n'existe pas si le site est dans l'état Catalogue à la date du paramètre
            ArchiveSite dernièreFinDEtatCatalogue = await archivesQuery.Where(a => a.Etat == TypeEtatSite.Ouvert).LastOrDefaultAsync();
            DateTime dateFinDEtatCatalogue;
            if (dernièreFinDEtatCatalogue == null || DateTime.Compare(dernièreFinDEtatCatalogue.Date, dernierDébutDEtatCatalogue.Date) < 0)
            {
                // le site est dans l'état Catalogue à la date
                dateFinDEtatCatalogue = DateNulle.Date;
            }
            else
            {
                // quand le site a quitté l'état Catalogue, dernièreFinDEtatCatalogue a été enregistrée dans les archives du site 
                dateFinDEtatCatalogue = dernièreFinDEtatCatalogue.Date;
            }
            return dateFinDEtatCatalogue;
        }

        /// <summary>
        /// Crée un tarif (catalogue avec données datées) à partir des archives passées en paramètre.
        /// </summary>
        /// <param name="aKeySite">un Site ou son UidRno</param>
        /// <param name="archivesProduit"></param>
        /// <param name="archivesCatégorie"></param>
        /// <param name="anciensSeulement">si vrai seules les archives antérieures à la date du catalogue complet sont utilisées</param>
        /// <returns></returns>
        public Catalogue Tarif(AKeyUidRno aKeySite,
            IEnumerable<ArchiveProduit> archivesProduit,
            IEnumerable<ArchiveCatégorie> archivesCatégorie,
            bool anciensSeulement)
        {
            if (anciensSeulement)
            {
                archivesProduit = _context.ArchiveProduit
                    .Join(archivesProduit,
                        a => new { a.Uid, a.Rno, a.No },
                        a => new { a.Uid, a.Rno, a.No },
                        (aDb, aParam) => new { aDb, aParam })
                    .GroupBy(aa => aa.aParam)
                    .Select(aa => new { archive = aa.Key, dateDansCatalogue = aa.Select(aa1 => aa1.aDb).OrderBy(ar => ar.Date).Last().Date })
                    .Where(ad => ad.archive.Date < ad.dateDansCatalogue)
                    .Select(ad => ad.archive);
                archivesProduit = archivesProduit
                    .GroupJoin(_context.ArchiveProduit,
                    a => new { a.Uid, a.Rno, a.No },
                    a => new { a.Uid, a.Rno, a.No },
                    (a, aas) => new { archive = a, dateDansCatalogue = aas.OrderBy(ar => ar.Date).Last().Date }
                    ).Where(ad => ad.archive.Date < ad.dateDansCatalogue)
                    .Select(ad => ad.archive);
                archivesCatégorie = archivesCatégorie
                    .GroupJoin(_context.ArchiveCatégorie,
                    a => new { a.Uid, a.Rno, a.No },
                    a => new { a.Uid, a.Rno, a.No },
                    (a, aas) => new { archive = a, dateDansCatalogue = aas.OrderBy(ar => ar.Date).Last().Date }
                    ).Where(ad => ad.archive.Date < ad.dateDansCatalogue)
                    .Select(ad => ad.archive);
            }
            List<ProduitData> produits = archivesProduit
                .Select(a => _produitService.CréeProduitDataAvecDate(a))
                .ToList();

            List<CatégorieData> catégories = archivesCatégorie
                .Select(a => _catégorieService.CréeCatégorieDataAvecDate(a))
                .ToList();

            Catalogue catalogue = new Catalogue
            {
                Uid = aKeySite.Uid,
                Rno = aKeySite.Rno,
                Produits = produits,
                Catégories = catégories
            };
            return catalogue;
        }

        /// <summary>
        /// retourne le catalogue complet du site actuellement en vigueur
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        private async Task<Catalogue> Catalogue(AKeyUidRno aKeySite, List<ProduitData> produits, List<CatégorieData> catégories)
        {
            Catalogue catalogue = new Catalogue
            {
                Uid = aKeySite.Uid,
                Rno = aKeySite.Rno,
                Produits = produits,
                Catégories = catégories,
                Date = await DateCatalogue(aKeySite, DateTime.Now)
            };
            return catalogue;
        }

        /// <summary>
        /// retourne le catalogue complet du site actuellement en vigueur
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public async Task<Catalogue> Complet(KeyUidRno keySite)
        {
            Site site = await _context.Site.Where(s => keySite.Uid == s.Uid && keySite.Rno == s.Rno).FirstOrDefaultAsync();
            if (site == null)
            {
                return null;
            }
            List<ProduitData> produits = await _produitService.ProduitDatas(keySite);
            List<CatégorieData> catégories = await _catégorieService.CatégorieDatas(keySite, null);
            Catalogue catalogue = await Catalogue(keySite, produits, catégories);
            return catalogue;
        }

        /// <summary>
        /// retourne le catalogue des disponibilités du site actuellement en vigueur
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public async Task<Catalogue> Disponibles(KeyUidRno keySite)
        {
            Site site = await _context.Site.Where(s => keySite.Uid == s.Uid && keySite.Rno == s.Rno).FirstOrDefaultAsync();
            if (site == null)
            {
                return null;
            }
            List<ProduitData> produits = await _produitService.ProduitDatasDisponibles(keySite);
            List<CatégorieData> catégories = await _catégorieService.CatégorieDatas(keySite, produits);
            Catalogue catalogue = await Catalogue(keySite, produits, catégories);
            return catalogue;
        }

        /// <summary>
        /// Retourne le catalogue complet si il n'y a pas de date ou si la date est antérieure à la dernière modification du catalogue,
        /// un catalogue sans produits sinon.
        /// Le catalogue retourné a une date égale à DateNulle si une modification du catalogue est en cours.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<Catalogue> Obsolète(Site site, DateTime? date)
        {
            Catalogue catalogue;
            DateTime dateCatalogue = await DateCatalogue(site, null);
            // si date est présent, l'Api client veut vérifier si son catalogue est à jour
            bool doitRetourner = !date.HasValue || DateTime.Compare(dateCatalogue, date.Value) > 0;
            if (doitRetourner)
            {
                List<ProduitData> produits = await _produitService.ProduitDatasDisponibles(site);
                List<CatégorieData> catégories = await _catégorieService.CatégorieDatas(site, null);
                catalogue = new Catalogue
                {
                    Uid = site.Uid,
                    Rno = site.Rno,
                    Produits = produits,
                    Catégories = catégories,
                    Date = dateCatalogue
                };
            }
            else
            {
                catalogue = new Catalogue();
                if (DateNulle.Egale(dateCatalogue))
                {
                    catalogue.Date = dateCatalogue;
                }
            }
            return catalogue;
        }

        /// <summary>
        /// archive les changements du catalogue depuis le début de la modification
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task ArchiveModifications(Site site)
        {
            DateTime maintenant = DateTime.Now;
            List<ArchiveSite> archives = await _context.ArchiveSite
                .Where(a => site.Uid == a.Uid && site.Rno == a.Rno && a.Etat == TypeEtatSite.Catalogue)
                .OrderBy(a => a.Date)
                .ToListAsync();
            ArchiveSite étatDébutModifCatalogue = archives.LastOrDefault();
            DateTime? débutModifCatalogue = null;
            if (étatDébutModifCatalogue != null)
            {
                débutModifCatalogue = étatDébutModifCatalogue.Date;
            }
            await _produitService.RésumeArchives(site, maintenant, débutModifCatalogue);
            await _catégorieService.RésumeArchives(site, maintenant, débutModifCatalogue);
        }
    }
}
