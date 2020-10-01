using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Partages.KeyParams
{    /// <summary>
     /// outil du KeyParamService de T, Tvue
     /// </summary>
     /// <typeparam name="T"></typeparam>
     /// <typeparam name="TVue"></typeparam>
    public interface IGéreArchiveUidRnoNo<T, TVue> : IGéreArchive<T, TVue> where T : AKeyUidRnoNo where TVue : AKeyUidRnoNo
    {

        /// <summary>
        /// remplace sans sauver les archives vérifiant le filtre et enregistrées depuis la date de début par des archives contenant les valeurs finales des données correspondantes
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="dateRésumé"></param>
        /// <param name="dateDébut"></param>
        /// <returns></returns>
        Task RésumeArchives(AKeyUidRno keySite, DateTime dateRésumé, DateTime? dateDébut);

        /// <summary>
        /// retourne la date de la dernière archive vérifiant le filtre et enregistrée avant la date de fin
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="jusquA">DateTime de fin</param>
        /// <returns></returns>
        Task<DateTime?> DateArchive(AKeyUidRno keySite, DateTime? jusquA);

    }

    /// <summary>
    /// outil du KeyUidRnoNoService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class GéreArchiveUidRnoNo<T, TVue, TArchive> : GéreArchive<T, TVue, TArchive>, IGéreArchiveUidRnoNo<T, TVue>
        where T : AKeyUidRnoNo where TVue : AKeyUidRnoNo where TArchive : AKeyUidRnoNo, IKeyArchive
    {
        public GéreArchiveUidRnoNo(DbSet<T> dbSet, DbSet<TArchive> dbSetArchive) : base(dbSet, dbSetArchive)
        {
        }

        protected override async Task<List<TArchive>> Archives(T donnée)
        {
            return await _dbSetArchive
                .Where(a => a.Uid == donnée.Uid && a.Rno == donnée.Rno && a.No == donnée.No)
                .ToListAsync();
        }

        /// <summary>
        /// remplace sans sauver les archives du site enregistrées depuis la date de début par des archives contenant les valeurs finales des données correspondantes
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="dateRésumé"></param>
        /// <param name="dateDébut"></param>
        /// <returns></returns>
        public async Task RésumeArchives(AKeyUidRno keySite, DateTime dateRésumé, DateTime? dateDébut)
        {
            IQueryable<TArchive> query = _dbSetArchive.Where(a => a.Uid == keySite.Uid && a.Rno == keySite.Rno);
            if (dateDébut != null)
            {
                query = query.Where(a => a.Date >= dateDébut.Value);
            }
            // archives vérifiant le filtre et enregistrées depuis la date de début
            List<TArchive> enregistrées = await query.ToListAsync();

            // clés des archives enregistrés
            IEnumerable<KeyParam> paramsDesEnregistrées = enregistrées.GroupBy(e => e.KeyParam).Select(ge => ge.Key);

            // on doit résumer s'il y a plus d'une archive pour la même donnée
            List<long> nosDesEnregistréesARésumer = enregistrées.GroupBy(a => a.No).Where(ge => ge.Count() > 1).Select(ge => ge.Key).ToList();

            if (!nosDesEnregistréesARésumer.Any())
            {
                return;
            }
            enregistrées = enregistrées.Where(a => nosDesEnregistréesARésumer.Where(no => a.No == no).Any()).ToList();

            // données ayant ces clés dans leur état en fin de modification
            List<T> données = await _dbSet
                .Where(t => t.Uid == keySite.Uid && t.Rno == keySite.Rno)
                .ToListAsync();
            données = données.Where(t => nosDesEnregistréesARésumer.Where(no => t.No == no).Any()).ToList();

            // archives crées à partir de ces données
            List<TArchive> résumés = données.Select(t => CréeArchiveComplet(t, dateRésumé)).ToList();

            // remplacement
            _dbSetArchive.RemoveRange(enregistrées);
            _dbSetArchive.AddRange(résumés);
        }

        /// <summary>
        /// retourne la date de la dernière archive du site enregistrée avant la date de fin
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="jusquA">DateTime de fin</param>
        /// <returns></returns>
        public async Task<DateTime?> DateArchive(AKeyUidRno keySite, DateTime? jusquA)
        {
            IQueryable<TArchive> query = _dbSetArchive.Where(a => a.Uid == keySite.Uid && a.Rno == keySite.Rno);
            if (jusquA != null)
            {
                query = query.Where(a => a.Date <= jusquA.Value);
            }
            // dernière archive vérifiant le filtre et enregistrée avant la date de fin
            TArchive dernière = await query.LastOrDefaultAsync();

            if (dernière == null)
            {
                return null;
            }
            return dernière.Date;
        }
    }

    public abstract class KeyUidRnoNoService<T, TVue> : KeyParamService<T, TVue, KeyParam>, IKeyUidRnoNoService<T, TVue>
       where T : AKeyUidRnoNo where TVue : AKeyUidRnoNo
    {
        public KeyUidRnoNoService(ApplicationContext context) : base(context)
        {
        } 

        public async Task<long> DernierNo(KeyParam param)
        {
            var données = _dbSet.Where(donnée => donnée.Uid == param.Uid && donnée.Rno == param.Rno);
            return await données.AnyAsync() ? await données.MaxAsync(donnée => donnée.No) : 0;
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
        /// remplace sans sauver les archives vérifiant le filtre et enregistrées depuis la date de début par des archives contenant les valeurs finales des données correspondantes
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="dateRésumé"></param>
        /// <param name="dateDébut"></param>
        /// <returns></returns>
        public async Task RésumeArchives(AKeyUidRno keySite, DateTime dateRésumé, DateTime? dateDébut)
        {
            IGéreArchiveUidRnoNo<T, TVue> géreArchive = (IGéreArchiveUidRnoNo<T, TVue>) _géreArchive;
            await géreArchive.RésumeArchives(keySite, dateRésumé, dateDébut);
        }

        /// <summary>
        /// retourne la date de la dernière archive vérifiant le filtre et enregistrée avant la date de fin
        /// </summary>
        /// <param name="keySite"></param>
        /// <param name="jusquA">DateTime de fin</param>
        /// <returns></returns>
        public async Task<DateTime?> DateArchive(AKeyUidRno keySite, DateTime? jusquA)
        {
            if (_géreArchive == null)
            {
                return null;
            }
            IGéreArchiveUidRnoNo<T, TVue> géreArchive = (IGéreArchiveUidRnoNo<T, TVue>) _géreArchive;
            return await géreArchive.DateArchive(keySite, jusquA);
        }
    }

}
