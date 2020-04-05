using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    /// <summary>
    /// Prix daté d'un produit
    /// </summary>
    public class PrixData : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du site
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du site
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du produit
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Date à laquelle le prix a été fixé
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Prix du produit
        /// </summary>
        public decimal Prix { get; set; }
    }
}
