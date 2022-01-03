using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IKeyUidRno
    {
        public int Rno { get; set; }
    }
    public class KeyUidRno : AKeyUidRno, IKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
    }
}
