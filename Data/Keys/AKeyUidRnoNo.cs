using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // base des modéles (et des vues) des données crées par un Role
    // classes dérivées: Produit, Commande, ... 
    public abstract class AKeyUidRnoNo : AKeyBase
    {
        public abstract string Uid { get; set; }
        public abstract int Rno { get; set; }
        public abstract long No { get; set; }

        public override string TexteKey
        {
            get
            {
                return Uid + Séparateur + Rno + Séparateur + No;
            }
        }

        public override bool AMêmeKey(AKeyBase donnée)
        {
            if (donnée is AKeyUidRnoNo)
            {
                AKeyUidRnoNo key = (donnée as AKeyUidRnoNo);
                return key.Uid == Uid && key.Rno == Rno && key.No == No;
            }
            return false;
        }

        public override bool CommenceKey(KeyParam param)
        {
            return Uid == param.Uid && Rno == param.Rno && No == param.No;
        }

        public override void CopieKey(KeyParam param)
        {
            Uid = param.Uid;
            Rno = param.Rno ?? 0;
            No = param.No ?? 0;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid, Rno = Rno, No = No };
        public override KeyParam KeyParamParent => new KeyParam { Uid = Uid, Rno = Rno };

        public static KeyUidRno KeyUidRno(AKeyUidRnoNo aKeyUidRnoNo)
        {
            return new KeyUidRno { Uid = aKeyUidRnoNo.Uid, Rno = aKeyUidRnoNo.Rno };
        }
    }
}
