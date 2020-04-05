using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Partages.KeyParams
{
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

    }
}
