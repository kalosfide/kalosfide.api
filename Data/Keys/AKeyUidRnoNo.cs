using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // base des modéles (et des vues) des données crées par un Role
    // classes dérivées: Produit, Commande, ... 
    public abstract class AKeyUidRnoNo : AKeyBase, IKeyUidRnoNo
    {
        public abstract string Uid { get; set; }
        public abstract int Rno { get; set; }
        public abstract long No { get; set; }

        public void CopieKey(AKeyUidRnoNo aKey)
        {
            Uid = aKey.Uid;
            Rno = aKey.Rno;
            No = aKey.No;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid, Rno = Rno, No = No };
        public override KeyParam KeyParamParent => new KeyParam { Uid = Uid, Rno = Rno };

        public static KeyUidRno KeyUidRno(AKeyUidRnoNo aKeyUidRnoNo)
        {
            return new KeyUidRno { Uid = aKeyUidRnoNo.Uid, Rno = aKeyUidRnoNo.Rno };
        }
    }
}
