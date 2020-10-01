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
                        if (param.Uid2 == null)
                        {
                            return _dbSet
                                .Where(key => key.Uid == param.Uid && key.Rno == param.Rno && key.No == param.No);
                        }
                        else
                        {
                            if (param.Rno2 == null)
                            {
                                return _dbSet
                                    .Where(key => key.Uid == param.Uid && key.Rno == param.Rno && key.No == param.No
                                        && key.Uid2 == param.Uid2);
                            }
                            else
                            {
                                if (param.No2 == null)
                                {
                                    return _dbSet
                                        .Where(key => key.Uid == param.Uid && key.Rno == param.Rno && key.No == param.No
                                            && key.Uid2 == param.Uid2 && key.Rno2 == param.Rno2);
                                }
                                else
                                {
                                    return _dbSet
                                        .Where(key => key.Uid == param.Uid && key.Rno == param.Rno && key.No == param.No
                                            && key.Uid2 == param.Uid2 && key.Rno2 == param.Rno2 && key.No2 == param.No2);
                                }
                            }
                        }
                    }
                }
            }
            return _dbSet;
        }
    }
}
