using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public interface ICLFLigneData
    {
        // données

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        DateTime? Date { get; set; }


        /// <summary>
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut de type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        string TypeCommande { get; set; }

        /// <summary>
        /// Quantité du produit
        /// </summary>
        decimal? Quantité { get; set; }

        /// <summary>
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        decimal? AFixer { get; set; }

    }

    /// <summary>
    /// Objet envoyé
    /// </summary>
    public class CLFLigneData : ICLFLigneData
    {

        /// <summary>
        /// No du Produit
        /// </summary>
        public long No { get; set; }

        // données

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        public DateTime? Date { get; set; }


        /// <summary>
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut de type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        public string TypeCommande { get; set; }


        /// <summary>
        /// Quantité du produit
        /// </summary>
        public decimal? Quantité { get; set; }

        /// <summary>
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        public decimal? AFixer { get; set; }

    }


    /// <summary>
    /// Objet reçu
    /// </summary>
    public class CLFLigne : AKeyUidRnoNo2, ICLFLigneData
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

        // données

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        public DateTime? Date { get; set; }


        /// <summary>
        /// Présent uniquement si le CLFDoc est une commande.
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut du type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        public string TypeCommande { get; set; }


        /// <summary>
        /// Quantité du produit
        /// </summary>
        public decimal? Quantité { get; set; }

        /// <summary>
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        public decimal? AFixer { get; set; }

        public CLFDoc CLFDoc { get; set; }
        public ArchiveProduit Produit { get; set; }

    }
}
