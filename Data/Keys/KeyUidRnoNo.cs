using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IKeyUidRnoNo : IKeyUidRno
    {
        public long No { get; set; }
    }
    public class KeyUidRnoNo : AKeyUidRnoNo, IKeyUidRnoNo
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public override long No { get; set; }
    }
}
