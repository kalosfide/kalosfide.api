using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    /// <summary>
    /// Ne contient que la date du catalogue
    /// </summary>
    public class ParamsVide
    {

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }
    public class ParamsKeyLigne : AKeyUidRnoNo2
    {
        /// <summary>
        /// Uid du Role et du Client du client et du document
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et du document
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document
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
        /// Date du catalogue
        /// </summary>
        public virtual DateTime DateCatalogue { get; set; }
    }
    public class ParamsFixeLigne : ParamsKeyLigne
    {
        /// <summary>
        /// Uid du Role et du Client du client et du document
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et du document
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document
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
        /// Date du catalogue
        /// </summary>
        public override DateTime DateCatalogue { get; set; }

        public decimal AFixer { get; set; }
    }

    public class ParamsKeyClient : AKeyUidRno
    {
        /// <summary>
        /// Uid du Role et du Client du client et du document
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et du document
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }

    public class ParamsKeyDoc : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du Role et du Client du client et du document
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et du document
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }
    }

    public class ParamsFiltreDoc : AKeyUidRno
    {
        /// <summary>
        /// Uid du client ou du site suivant l'action appelée.
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du client ou du site suivant l'action appelée.
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// Type des documents à retourner
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Index dans la liste des documents passant le filtre du premier document à retourner
        /// </summary>
        public int? I0 { get; set; }

        /// <summary>
        /// Nombre de documents à retourner
        /// </summary>
        public int? Nb { get; set; }

        /// <summary>
        /// Date minimum des documents à retourner
        /// </summary>
        public DateTime? DateMin { get; set; }

        /// <summary>
        /// Date maximum des documents à retourner
        /// </summary>
        public DateTime? DateMax { get; set; }
    }

}