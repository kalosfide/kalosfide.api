using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IKeyUid
    {
        public string Uid { get; set; }
    }
    public class KeyUid : AKeyUid, IKeyUid
    {
        public override string Uid { get; set; }
    }
}
