using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{

    /// <summary>
    /// Objet envoyé
    /// </summary>
    public class CLFLigneData
    {
        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        // données

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        public DateTime Date { get; set; }


        /// <summary>
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut de type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        public TypeCommande TypeCommande { get; set; }


        /// <summary>
        /// Quantité du produit
        /// </summary>
        public decimal? Quantité { get; set; }

        /// <summary>
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        public decimal? AFixer { get; set; }

        public static CLFLigneData LigneData(LigneCLF ligneCLF)
        {
            CLFLigneData data = new CLFLigneData
            {
                ProduitId = ligneCLF.ProduitId,
                Date = ligneCLF.Date,
                TypeCommande = ligneCLF.TypeCommande,
                Quantité = ligneCLF.Quantité,
            };
            return data;
        }

        public static CLFLigneData LigneDataAvecAFixer(LigneCLF ligneCLF)
        {
            CLFLigneData data = CLFLigneData.LigneData(ligneCLF);
            data.AFixer = ligneCLF.AFixer;
            return data;
        }

    }


    /// <summary>
    /// Objet reçu
    /// </summary>
    public class CLFLigne : IKeyLigneSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public uint No { get; set; }

        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        // données

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        public DateTime Date { get; set; }


        /// <summary>
        /// Présent uniquement si le CLFDoc est une commande.
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut du type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        public TypeCommande TypeCommande { get; set; }


        /// <summary>
        /// Quantité du produit
        /// </summary>
        public decimal? Quantité { get; set; }

        /// <summary>
        /// Présent uniquement
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        public decimal? AFixer { get; set; }

    }
}
