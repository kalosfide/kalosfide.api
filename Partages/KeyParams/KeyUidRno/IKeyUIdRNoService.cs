using KalosfideAPI.Data.Keys;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyUidRnoService<T, TVue> : IKeyParamService<T, TVue, KeyParam> where T: AKeyUidRno where TVue: AKeyUidRno
    {
        Task<int> DernierNo(string uid);
    }
}
