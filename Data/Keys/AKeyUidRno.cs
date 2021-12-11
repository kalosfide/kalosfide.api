using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // base des modéles (et des vues) des données crées par un Utilisateur
    // classes dérivées: Role, Administrateur, Client, Fournisseur
    public abstract class AKeyUidRno : AKeyBase, IKeyUidRno
    {
        public abstract string Uid { get; set; }
        public abstract int Rno { get; set; }

        public void CopieKey(AKeyUidRno aKey)
        {
            Uid = aKey.Uid;
            Rno = aKey.Rno;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid, Rno = Rno };
        public override KeyParam KeyParamParent => new KeyParam { Uid = Uid };
    }
}
