using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    // base des modéles (et des vues) des données crées par un Role
    // classes dérivées: Produit, Commande, ... 
    public abstract class AKeyDocSansType : IKeyDocSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public abstract uint Id { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public abstract uint No { get; set; }

        public void CopieKey(AKeyDocSansType aKey)
        {
            Id = aKey.Id;
            No = aKey.No;
        }
    }
}
