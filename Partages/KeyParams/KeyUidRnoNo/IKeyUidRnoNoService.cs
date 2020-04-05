using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyUidRnoNoService<T, TVue> : IKeyParamService<T, TVue, KeyParam> where T : AKeyUidRnoNo where TVue: AKeyUidRnoNo
    {
        Task<long> DernierNo(KeyParam param);

        Task<Site> SiteDeDonnée(T donnée);
    }
}
