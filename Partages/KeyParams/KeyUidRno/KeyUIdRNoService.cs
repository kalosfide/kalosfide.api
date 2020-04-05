using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
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

    }
}
