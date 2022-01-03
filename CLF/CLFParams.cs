using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    /// <summary>
    /// Ne contient que la date du catalogue du site.
    /// </summary>
    public class ParamsVide
    {

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient l'Id du client, le No du document et la date du catalogue du site.
    /// </summary>
    public class ParamsKeyDoc: IKeyDocSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public uint No { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient l'Id du client et la date du catalogue de l'utilisateur.
    /// </summary>
    public class ParamsKeyClient
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime? DateCatalogue { get; set; }
    }

    /// <summary>
    /// Contient l'Id du client, le No du document, le ProduitId de la ligne et la date du catalogue de l'utilisateur.
    /// </summary>
    public class ParamsSupprimeLigne: ParamsKeyDoc
    {
        /// <summary>
        /// Id du Produit de la ligne à supprimer
        /// </summary>
        public uint ProduitId { get; set; }
    }

    /// <summary>
    /// Contient l'Id du client, le No du document, le ProduitId et la Date de la ligne et la date du catalogue de l'utilisateur.
    /// </summary>
    public class ParamsKeyLigne : IKeyLigneSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public uint No { get; set; }


        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        /// <summary>
        /// Quand une ligne est ajoutée à un bon, elle a une date nulle (définie dans CLFService).
        /// Quand un bon de commande est envoyé, sa date est fixée et ses lignes prennent la date du bon.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons ont une date.
        /// </summary>
        public DateTime Date { get; set; }

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
    /// Contient l'Id du client et la liste des No des documents à synthétiser.
    /// </summary>
    public class ParamsSynthèse
    {
        /// <summary>
        /// Id du Client du client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Liste des No des documents à synthétiser
        /// </summary>
        public List<long> NoDocs { get; set; }

    }

    public class ParamsFiltreDoc
    {
        /// <summary>
        /// Id du Client ou du Site
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Type des documents à retourner.
        /// Si présent, Types est inutilisé.
        /// </summary>
        public TypeCLF? Type { get; set; }

        /// <summary>
        /// Types possibles des documents à retourner. Seules les deux premiers sont utilisés.
        /// Inutilisé, si Type est présent.
        /// </summary>
        public TypeCLF[] Type1Ou2 { get; set; }

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

    public class ParamsChercheDoc
    {
        /// <summary>
        /// Id du Site
        /// </summary>
        public uint SiteId { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public uint No { get; set; }

        /// <summary>
        /// Type du document recherché (livraison ou facture)
        /// </summary>
        public TypeCLF Type { get; set; }
    }

}