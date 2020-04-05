using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // base des modéles (et des vues) des données crées par un Utilisateur
    // classes dérivées: Role, Administrateur, Client, Fournisseur
    public abstract class AKeyUidRno : AKeyBase
    {
        public abstract string Uid { get; set; }
        public abstract int Rno { get; set; }

        public override string TexteKey
        {
            get
            {
                return Uid + Séparateur + Rno;
            }
        }
        public override bool AMêmeKey(AKeyBase donnée)
        {
            if (donnée is AKeyUidRno)
            {
                AKeyUidRno key = (donnée as AKeyUidRno);
                return key.Uid == Uid && key.Rno == Rno;
            }
            return CommenceKey(donnée.KeyParam);
        }

        public override bool CommenceKey(KeyParam param)
        {
            return Uid == param.Uid && Rno == param.Rno;
        }

        public override void CopieKey(KeyParam param)
        {
            Uid = param.Uid;
            Rno = param.Rno?? 0;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid, Rno = Rno };
        public override KeyParam KeyParamParent => new KeyParam { Uid = Uid };
    }
}
