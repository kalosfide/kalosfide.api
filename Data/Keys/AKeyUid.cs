using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // classes dérivées: Utilisateur
    public abstract class AKeyUid : AKeyBase
    {
        public abstract string Uid { get; set; }
        public override string TexteKey
        {
            get
            {
                return Uid;
            }
        }
        public override bool AMêmeKey(AKeyBase donnée)
        {
            if (donnée is AKeyUid)
            {
                return (donnée as AKeyUid).Uid == Uid;
            }
            return false;
        }

        public override bool CommenceKey(KeyParam param)
        {
            return Uid == param.Uid;
        }

        public override void CopieKey(KeyParam param)
        {
            Uid = param.Uid;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid };
        public override KeyParam KeyParamParent => null;
    }
}
