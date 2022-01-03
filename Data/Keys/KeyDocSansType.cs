using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IKeyDocSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public uint No { get; set; }
    }
    public class KeyDocSansType : AKeyDocSansType, IKeyDocSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public override uint Id { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public override uint No { get; set; }
    }
}
