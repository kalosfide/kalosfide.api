using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // classes dérivées: Utilisateur
    public abstract class AKeyUid : AKeyBase, IKeyUid
    {
        public abstract string Uid { get; set; }

        public override KeyParam KeyParam => new KeyParam { Uid = Uid };
        public override KeyParam KeyParamParent => null;
    }
}
