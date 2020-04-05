using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Factures
{
    #region Lecture

    public class DétailAFacturer
    {

        /// <summary>
        /// No du produit
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// ALivrer du produit à facturer
        /// </summary>
        public decimal ALivrer { get; set; }

        /// <summary>
        /// ALivrer du produit à facturer
        /// </summary>
        public decimal? AFacturer { get; set; }

    }

    public class CommandeAFacturer
    {

        /// <summary>
        /// No de la commande
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// No de la livraison
        /// </summary>
        public long LivraisonNo { get; set; }

        public List<DétailAFacturer> Details { get; set; }

    }

    public class AFacturerDUnClient : AKeyUidRno
    {
        /// <summary>
        /// Uid du client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du client
        /// </summary>
        public override int Rno { get; set; }

        public List<CommandeAFacturer> Commandes { get; set; }
    }

    public class LivraisonAFacturer
    {
        /// <summary>
        /// No de la livraison
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// Date de la Livraison
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class AFacturer
    {
        /// <summary>
        /// Liste des listes des commandes livrées non facturées regroupées par client
        /// </summary>
        public List<AFacturerDUnClient> AFacturerParClient { get; set; }

        /// <summary>
        /// Liste des No et Date des livraisons des commandes livrées non facturées
        /// </summary>
        public List<LivraisonAFacturer> Livraisons { get; set; }

        /// <summary>
        /// Numéro de la prochaine facture
        /// </summary>
        public long NoProchaineFacture { get; set; }
    }

    #endregion // Lecture

    /// <summary>
    /// représente une facture d'un client
    /// </summary>
    public class FactureVue : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du site et du fournisseur
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du site et du fournisseur
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No de la facture
        /// </summary>
        public override long No { get; set; }

        // données

        /// <summary>
        /// Uid du client
        /// </summary>
        public string Uid2 { get; set; }

        /// <summary>
        /// Rno du client
        /// </summary>
        public int Rno2 { get; set; }

        public DateTime? Date { get; set; }

        /// <summary>
        /// liste des cumuls par produits des DétailCommande des commandes livrées non facturées
        /// </summary>
        public List<DétailFactureData> Détails { get; set; }

        /// <summary>
        /// liste des numéros de commande, des numéros et dates de livraison des commandes livrées non facturées
        /// </summary>
        public List<FactureCommandeData> Commandes { get; set; }
    }

    public class FactureClientVue : AKeyUidRno
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
        /// liste des cumuls par produits des DétailCommande des commandes livrées non facturées
        /// </summary>
        public List<DétailFactureData> Details { get; set; }

        /// <summary>
        /// liste des numéros de commande, des numéros et dates de livraison des commandes livrées non facturées
        /// </summary>
        public List<FactureCommandeData> Commandes { get; set; }

    }

    public class FactureCommandeData
    {

        /// <summary>
        /// No de la commande
        /// </summary>
        public long No { get; set; }
        public long NoLivraison { get; set; }

        /// <summary>
        /// date de la livraison
        /// </summary>
        public DateTime Date { get; set; }

    }

    public class DétailFactureData
    {
        /// <summary>
        /// Uid du client (présent en écriture)
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// Rno du client (présent en écriture)
        /// </summary>
        public int Rno { get; set; }

    }
}
