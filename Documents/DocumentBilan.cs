using KalosfideAPI.Catalogues;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Documents
{

    public class BilanProduit
    {
        /// <summary>
        /// No du produit
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// Date de la dernière modification du produit
        /// </summary>
        public DateTime? Date { get; set; }

        public Decimal Total { get; set; }
    }

    public class DocumentBilan : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du client
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Facture
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Date de la Facture
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Présent si la facture est chargée pour le document.
        /// </summary>
        public List<BilanProduit> Produits { get; set; }

        /// <summary>
        ///  Nombre de bilans-produits. Présent si la facture est chargée pour la liste.
        /// </summary>
        public int? Lignes { get; set; }

        /// <summary>
        /// Montant total de la facture. Présent si la facture est chargée pour la liste.
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// Catalogue en vigueur à la date du document. Présent si la facture est chargée pour le document.
        /// </summary>
        public Catalogue Tarif { get; set; }

    }
}
