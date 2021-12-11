using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{

    /// <summary>
    /// outil du KeyUidRnoService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class GéreArchiveUidRno<T, TVue, TArchive> : GéreArchive<T, TVue, TArchive>
        where T : AKeyUidRno where TVue : AKeyUidRno where TArchive : AKeyUidRno, IAvecDate
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
        public GéreArchiveUidRno(
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
    }

    public abstract class KeyUidRnoService<T, TVue> : KeyParamService<T, TVue>, IKeyUidRnoService<T, TVue>
        where T : AKeyUidRno where TVue: AKeyUidRno
    {
        public KeyUidRnoService(ApplicationContext context) : base(context)
        {
        }

        protected override void CopieKey(TVue de, T vers)
        {
            vers.CopieKey(de);
        }

        public async Task<int> DernierNo(string uid)
        {
            var données = _dbSet.Where(donnée => donnée.Uid == uid);
            return await données.AnyAsync() ? await données.MaxAsync(donnée => donnée.Rno) : 0;
        }

        public async Task<T> Lit(AKeyUidRno key)
        {
            return await _dbSet
                .Where(d => d.Uid == key.Uid && d.Rno == key.Rno)
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
                    return _dbSet.Where(key => key.Uid == param.Uid && key.Rno == param.Rno);
                }
            }
            return _dbSet;
        }

    }
}
