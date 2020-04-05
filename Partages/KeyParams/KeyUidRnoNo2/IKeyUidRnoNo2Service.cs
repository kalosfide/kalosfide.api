using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyUidRnoNo2Service<T, TVue> : IKeyParamService<T, TVue, KeyParam> where T : AKeyUidRnoNo2 where TVue: AKeyUidRnoNo2
    {
    }
}
