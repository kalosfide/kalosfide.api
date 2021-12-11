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

        public bool Egale(KeyParam param)
        {
            return Uid == param.Uid && Rno == param.Rno && No == param.No && Date == param.Date && Uid2 == param.Uid2 && Rno2 == param.Rno2 && No2 == param.No2;
        }

        static public KeyUid CréeKeyUid(KeyParam param)
        {
            if (param.Uid == null)
            {
                return null;
            }
            return new KeyUid
            {
                Uid = param.Uid
            };
        }
        static public KeyUidRno CréeKeyUidRno(KeyParam param)
        {
            if (param.Uid == null || param.Rno == null)
            {
                return null;
            }
            return new KeyUidRno
            {
                Uid = param.Uid,
                Rno = param.Rno.Value
            };
        }
        static public KeyUidRnoNo CréeKeyUidRnoNo(KeyParam param)
        {
            if (param.Uid == null || param.Rno == null || param.No == null)
            {
                return null;
            }
            return new KeyUidRnoNo
            {
                Uid = param.Uid,
                Rno = param.Rno.Value,
                No = param.No.Value
            };
        }

    }
}
