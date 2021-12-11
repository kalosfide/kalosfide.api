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
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient la KeyUidRnoNo du document et la date du catalogue de l'utilisateur.
    /// </summary>
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
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient la KeyUidRno du client et la date du catalogue de l'utilisateur.
    /// </summary>
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
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient la KeyUidRnoNo du document, le No2 de la ligne et la date du catalogue de l'utilisateur.
    /// </summary>
    public class ParamsSupprimeLigne: ParamsKeyDoc
    {
        /// <summary>
        /// No du Produit de la ligne à supprimer
        /// </summary>
        public long No2 { get; set; }
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
        /// Quand une ligne est ajoutée à un bon, elle a une date nulle (définie dans CLFService).
        /// Quand un bon de commande est envoyé, sa date est fixée et ses lignes prennent la date du bon.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons ont une date.
        /// </summary>
        public override DateTime Date { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public virtual DateTime? DateCatalogue { get; set; }
    }
    public class ParamsFixeLigne : ParamsKeyLigne
    {
        public decimal AFixer { get; set; }
    }

    /// <summary>
    /// A la key d'un client et contient la liste des No des documents à synthétiser
    /// </summary>
    public class ParamsSynthèse : AKeyUidRno
    {
        /// <summary>
        /// Uid du Client du client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Client du client
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// Liste des No des documents à synthétiser
        /// </summary>
        public List<long> NoDocs { get; set; }

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
        /// Type des documents à retourner.
        /// Peut être "C" ou "L" ou "F" ou une chaîne "L F"
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

    public class ParamsChercheDoc : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du site.
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du site.
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Type du document recherché (livraison ou facture)
        /// </summary>
        public string Type { get; set; }
    }

}