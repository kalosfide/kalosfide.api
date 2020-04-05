using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public class ProduitBilan
    {
        public string Type { get; set; }
        /// <summary>
        /// Nombre de fois que le produit figure dans les livraisons. Uniquement dans le catalogue complet
        /// </summary>
        public int Nb { get; set; }

        /// <summary>
        /// Quantité totale du produit dans les livraisons. Uniquement dans le catalogue complet
        /// </summary>
        public decimal Quantité { get; set; }
    }
}
