using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
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
        where T : AKeyUidRno where TVue : AKeyUidRno where TArchive : AKeyUidRno, IKeyArchive
    {
        public GéreArchiveUidRno(DbSet<T> dbSet, DbSet<TArchive> dbSetArchive) : base(dbSet, dbSetArchive)
        {
        }

        protected override async Task<List<TArchive>> Archives(T donnée)
        {
            return await _dbSetArchive
                .Where(a => a.Uid == donnée.Uid && a.Rno == donnée.Rno)
                .ToListAsync();
        }
    }
    public abstract class KeyUidRnoService<T, TVue> : KeyParamService<T, TVue, KeyParam>, IKeyUidRnoService<T, TVue>
        where T : AKeyUidRno where TVue: AKeyUidRno
    {
        public KeyUidRnoService(ApplicationContext context) : base(context)
        {
        }

        public async Task<int> DernierNo(string uid)
        {
            var données = _dbSet.Where(donnée => donnée.Uid == uid);
            return await données.AnyAsync() ? await données.MaxAsync(donnée => donnée.Rno) : 0;
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
