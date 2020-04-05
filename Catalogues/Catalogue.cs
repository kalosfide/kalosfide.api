using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public class Catalogue : AKeyUidRno
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
        /// Date du Catalogue n'existe pas si la modification est en cours
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Données des produits du site à la date du catalogue
        /// </summary>
        public List<ProduitData> Produits { get; set; }

        /// <summary>
        /// Catégories du site à la date du catalogue
        /// </summary>
        public List<CatégorieData> Catégories { get; set; }

        /// <summary>
        /// Prix datés
        /// Présent si le catalogue doit être complété
        /// </summary>
        public List<PrixData> Prix { get; set; }

    }
}
