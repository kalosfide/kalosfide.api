using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Partages.KeyParams
{
    public abstract class KeyUidRnoNo2Service<T, TVue> : KeyParamService<T, TVue, KeyParam>, IKeyUidRnoNo2Service<T, TVue>
       where T : AKeyUidRnoNo2 where TVue : AKeyUidRnoNo2
    {
        public KeyUidRnoNo2Service(ApplicationContext context) : base(context)
        {
        }

        public Data.Keys.KeyUidRnoNo Key2(Data.Keys.AKeyUidRnoNo2 key)
        {
            return new Data.Keys.KeyUidRnoNo
            {
                Uid = key.Uid2,
                Rno = key.Rno2,
                No = key.No
            };
        }
    }
}
