using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.DétailCommandes
{

    public interface IData
    {
        string TypeCommande { get; set; }
        decimal? Demande { get; set; }
        decimal? ALivrer { get; set; }

        DateTime? Date { get; set; }
    }

    public class DétailCommandeVue : AKeyUidRnoNo2, IData
    {
        /// <summary>
        /// Uid du Role et du Client du client et de la Commande
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et de la Commande
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Commande
        /// </summary>
        public override long No { get; set; }


        /// <summary>
        /// Uid du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        public override string Uid2 { get; set; }

        /// <summary>
        /// Rno du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        public override int Rno2 { get; set; }

        /// <summary>
        /// No du Produit
        /// </summary>
        public override long No2 { get; set; }

        // données
        public string TypeCommande { get; set; }
        public decimal? Demande { get; set; }
        public decimal? ALivrer { get; set; }
        public decimal? AFacturer { get; set; }

        public DateTime? Date { get; set; }

    }
    public class DétailCommandeData : IData
    {
        public long No { get; set; } // No du produit
        public string TypeCommande { get; set; }
        public decimal? Demande { get; set; }
        public decimal? ALivrer { get; set; }
        public decimal? AFacturer { get; set; }

        public DateTime? Date { get; set; }
    }
}
