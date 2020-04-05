using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFClientsDocs : AKeyUidRno
    {
        /// <summary>
        /// Uid du Client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Client
        /// </summary>
        public override int Rno { get; set; }

        public int Envoyés { get; set; }

        public int Prêts { get; set; }
    }
}
