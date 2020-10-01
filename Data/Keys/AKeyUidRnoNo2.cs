using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public abstract class AKeyUidRnoNo2 : AKeyBase
    {
        public abstract string Uid { get; set; }
        public abstract int Rno { get; set; }
        public abstract long No { get; set; }
        public abstract string Uid2 { get; set; }
        public abstract int Rno2 { get; set; }
        public abstract long No2 { get; set; }

        public override string TexteKey
        {
            get
            {
                return Uid + Séparateur + Rno + Séparateur + No + Séparateur + Uid2 + Séparateur + Rno2 + Séparateur + No2;
            }
        }

        public override bool AMêmeKey(AKeyBase donnée)
        {
            if (donnée is AKeyUidRnoNo2)
            {
                AKeyUidRnoNo2 key = donnée as AKeyUidRnoNo2;
                return Uid == key.Uid && Rno == key.Rno && No == key.No && Uid2 == key.Uid2 && Rno2 == key.Rno2 && No2 == key.No2;
            }
            return false;
        }

        public override bool CommenceKey(KeyParam param)
        {
            return Uid == param.Uid && Rno == param.Rno && No == param.No && Uid2 == param.Uid2 && Rno == param.Rno2 && No2 == param.No2;
        }

        public override void CopieKey(KeyParam param)
        {
            Uid = param.Uid;
            Rno = param.Rno ?? 0;
            No = param.No ?? 0;
            Uid2 = param.Uid2;
            Rno2 = param.Rno2 ?? 0;
            No2 = param.No2 ?? 0;
        }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid, Rno = Rno, No = No, Uid2 = Uid2, Rno2 = Rno2, No2 = No2 };
        public override KeyParam KeyParamParent => new KeyParam { Uid = Uid, Rno = Rno, No = No };

        public static KeyUidRno KeyUidRno_1(AKeyUidRnoNo2 keyUidRnoNo2)
        {
            return new KeyUidRno { Uid = keyUidRnoNo2.Uid, Rno = keyUidRnoNo2.Rno };
        }

        public static KeyUidRno KeyUidRno_2(AKeyUidRnoNo2 keyUidRnoNo2)
        {
            return new KeyUidRno { Uid = keyUidRnoNo2.Uid2, Rno = keyUidRnoNo2.Rno2 };
        }

        public static KeyUidRnoNo KeyUidRnoNo_1(AKeyUidRnoNo2 keyUidRnoNo2)
        {
            return new KeyUidRnoNo { Uid = keyUidRnoNo2.Uid, Rno = keyUidRnoNo2.Rno, No = keyUidRnoNo2.No };
        }

        public static KeyUidRnoNo KeyUidRnoNo_2(AKeyUidRnoNo2 keyUidRnoNo2)
        {
            return new KeyUidRnoNo { Uid = keyUidRnoNo2.Uid2, Rno = keyUidRnoNo2.Rno2, No = keyUidRnoNo2.No2 };
        }

    }
}

