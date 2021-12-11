using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public class KeyUidRnoDate : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public DateTime Date { get; set; }
    }
}
