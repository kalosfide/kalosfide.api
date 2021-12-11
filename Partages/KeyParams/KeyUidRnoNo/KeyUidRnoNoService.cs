using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace KalosfideAPI.Partages.KeyParams
{    /// <summary>
     /// outil du KeyParamService de T, Tvue
     /// </summary>
     /// <typeparam name="T"></typeparam>
     /// <typeparam name="TVue"></typeparam>
    public interface IGéreArchiveUidRnoNo<T, TVue> : IGéreArchive<T, TVue> where T : AKeyUidRnoNo where TVue : AKeyUidRnoNo
    {

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="keySite">un objet ayant la key du site</param>
        /// <param name="dateDébut">date à laquelle la modification des données a commencé</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        Task<bool> TermineModification(AKeyUidRno keySite, DateTime dateDébut, DateTime dateFin);

    }

    /// <summary>
    /// outil du KeyUidRnoNoService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class GéreArchiveUidRnoNo<T, TVue, TArchive> : GéreArchive<T, TVue, TArchive>, IGéreArchiveUidRnoNo<T, TVue>
        where T : AKeyUidRnoNo, IAvecDate where TVue : AKeyUidRnoNo where TArchive : AKeyUidRnoNo, IAvecDate
    {
        /// <summary>
        /// Gestionnaire des archives d'un type de données.
        /// </summary>
        /// <param name="dbSet">DbSet des données</param>
        /// <param name="query">requête retournant les données incluant leurs archives</param>
        /// <param name="archives">fonction retournant les archives d'une donnée</param>
        /// <param name="dbSetArchive">DbSet des archives</param>
        /// <param name="queryArchive">requête retournant les archives incluant leur donnée</param>
        /// <param name="donnée">fonction retournant la donnée d'une archive</param>
        public GéreArchiveUidRnoNo(
            DbSet<T> dbSet,
            IIncludableQueryable<T, ICollection<TArchive>> query,
            Func<T, ICollection<TArchive>> archives,
            DbSet<TArchive> dbSetArchive,
            IIncludableQueryable<TArchive, T> queryArchive,
            Func<TArchive, T> donnée
            ) : base(dbSet, query, archives, dbSetArchive, queryArchive, donnée)
        {
        }

        protected override void CopieKey(T de, TArchive vers)
        {
            vers.CopieKey(de);
        }

        private TArchive CréeArchive(AKeyUidRno keySite, long no, DateTime date)
        {
            TArchive archive = CréeArchive();
            archive.Uid = keySite.Uid;
            archive.Rno = keySite.Rno;
            archive.No = no;
            archive.Date = date;
            return archive;
        }

        /// <summary>
        /// Copie tous les champs non nuls sauf la clé.
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="archive"></param>
        protected abstract void CopieArchiveDansArchive(TArchive de, TArchive vers);

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="keySite">un objet ayant la key du site</param>
        /// <param name="dateDébut">date à laquelle la modification des données a commencé</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        public async Task<bool> TermineModification(AKeyUidRno keySite, DateTime dateDébut, DateTime dateFin)
        {
            List<TArchive> résumés = new List<TArchive>();
            List<T> donnéesModifiées = new List<T>();

            List<TArchive> nouvellesArchives = await _queryArchive
                // recherche les archives enregistrées depuis le début de la modification
                .Where(a => a.Uid == keySite.Uid && a.Rno == keySite.Rno && a.Date > dateDébut)
                .ToListAsync();
            if (nouvellesArchives.Count() == 0)
            {
                return false;
            }
            List<List<TArchive>> archivesParDonnée = nouvellesArchives
                // groupe par donnée
                .GroupBy(a => a.No)
                .Select(g => g.ToList())
                .ToList();

            archivesParDonnée.ForEach(archives =>
            {
                T donnée = _donnée(archives.First());
                donnée.Date = dateFin;
                donnéesModifiées.Add(donnée);
                TArchive résumé;
                résumé = archives.OrderBy(a => a.Date).Aggregate(
                    CréeArchive(keySite, donnée.No, dateFin),
                    (r, a) =>
                    {
                        CopieArchiveDansArchive(a, r);
                        return r;
                    });
                résumés.Add(résumé);
            });
            _dbSet.UpdateRange(donnéesModifiées);
            _dbSetArchive.RemoveRange(nouvellesArchives);
            _dbSetArchive.AddRange(résumés);
            return true;
        }
    }

    public abstract class KeyUidRnoNoService<T, TVue> : KeyParamService<T, TVue>, IKeyUidRnoNoService<T, TVue>
       where T : AKeyUidRnoNo, IAvecDate where TVue : AKeyUidRnoNo
    {
        public KeyUidRnoNoService(ApplicationContext context) : base(context)
        {
        }

        protected override void CopieKey(TVue de, T vers)
        {
            vers.CopieKey(de);
        }

        public async Task<long> DernierNo(AKeyUidRnoNo aKey)
        {
            var données = _dbSet.Where(donnée => donnée.Uid == aKey.Uid && donnée.Rno == aKey.Rno);
            return await données.AnyAsync() ? await données.MaxAsync(donnée => donnée.No) : 0;
        }

        public async Task<T> Lit(AKeyUidRnoNo key)
        {
            return await _dbSet
                .Where(d => d.Uid == key.Uid && d.Rno == key.Rno && d.No == key.No)
                .FirstOrDefaultAsync();
        }

        public async Task<Site> SiteDeDonnée(T donnée)
        {
            return await _context.Site
                .Where(s => s.Uid == donnée.Uid && s.Rno == donnée.Rno)
                .FirstOrDefaultAsync();
        }

        protected override IQueryable<T> DbSetFiltré(KeyParam param)
        {
            if (param != null && param.Uid != null)
            {
                if (param.Rno == null)
                {
                    return _dbSet.Where(key => key.Uid == param.Uid);
                }
                else
                {
                    if (param.No == null)
                    {
                        return _dbSet
                            .Where(key => key.Uid == param.Uid && key.Rno == param.Rno);
                    }
                    else
                    {
                        return _dbSet
                            .Where(key => key.Uid == param.Uid && key.Rno == param.Rno && key.No == param.No);
                    }
                }
            }
            return _dbSet;
        }

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les archives créées enregistrées depuis la date de début.
        /// Si plusieurs de ces archives concernent la même donnée, les remplace par une seule archive
        /// résumant les modifications que ces archives ont enregistré.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="site">le site</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        public async Task<bool> TermineModification(Site site, DateTime dateFin)
        {
            IGéreArchiveUidRnoNo<T, TVue> géreArchive = (IGéreArchiveUidRnoNo<T, TVue>) _géreArchive;
            DateTime dateDébut = site.Archives
                .Where(a => a.Ouvert == false)
                .OrderBy(a => a.Date)
                .Select(a => a.Date)
                .Last();
            return await géreArchive.TermineModification(site, dateDébut, dateFin);
        }
    }

}
