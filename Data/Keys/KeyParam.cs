    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public class KeyParam
    {
        public string Uid { get; set; }
        public int? Rno { get; set; }
        public long? No { get; set; }
        public DateTime? Date { get; set; }
        public string Uid2 { get; set; }
        public int? Rno2 { get; set; }
        public long? No2 { get; set; }

        public KeyParam()
        {

        }
        public KeyParam(KeyParam param)
        {
            Uid = param.Uid;
            Rno = param.Rno ?? param.Rno;
            No = param.No ?? param.No;
            Date = param.Date ?? param.Date;
            Uid2 = param.Uid2;
            Rno2 = param.Rno2 ?? param.Rno2;
            No2 = param.No2 ?? param.No2;
        }

        public bool Egale(KeyParam param)
        {
            return Uid == param.Uid && Rno == param.Rno && No == param.No && Date == param.Date && Uid2 == param.Uid2 && Rno2 == param.Rno2 && No2 == param.No2;
        }

        public KeyUid CréeKeyUid()
        {
            if (Uid == null)
            {
                return null;
            }
            return new KeyUid
            {
                Uid = Uid
            };
        }
        public KeyUidRno CréeKeyUidRno()
        {
            if (Uid == null || Rno == null)
            {
                return null;
            }
            return new KeyUidRno
            {
                Uid = Uid,
                Rno = Rno ?? 0
            };
        }
        public KeyUidRnoNo CréeKeyUidRnoNo()
        {
            if (Uid == null || Rno == null || No == null)
            {
                return null;
            }
            return new KeyUidRnoNo
            {
                Uid = Uid,
                Rno = Rno ?? 0,
                No = No ?? 0
            };
        }

    }
}
