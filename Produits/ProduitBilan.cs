using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public class ProduitBilan
    {
        /// <summary>
        /// Type des documents dont on fait le bilan.
        /// </summary>
        public TypeCLF Type { get; set; }
        /// <summary>
        /// Nombre de fois que le produit figure dans les documents. Uniquement dans le catalogue complet
        /// </summary>
        public int Nb { get; set; }

        /// <summary>
        /// Quantité totale du produit dans les documents. Uniquement dans le catalogue complet
        /// </summary>
        public decimal Quantité { get; set; }

        /// <summary>
        /// Coût total du produit dans les documents. Uniquement dans le catalogue complet
        /// </summary>
        public decimal Coût { get; set; }

        /// <summary>
        /// Présent et faux si l'un des documents contient des lignes dont le coût n'est pas calculable.
        /// </summary>
        public bool? Incomplet { get; set; }
    }
}
