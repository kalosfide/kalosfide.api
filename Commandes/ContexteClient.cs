using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{
    public class ParamsEditeDétail
    {
        /// <summary>
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }
    public class ParamsSupprimeDétail : AKeyUidRnoNo2
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

        /// <summary>
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }
    public class ParamsCréeCommande : AKeyUidRno
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
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }
    public class ParamsSupprimeCommande : AKeyUidRnoNo
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
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }
}
