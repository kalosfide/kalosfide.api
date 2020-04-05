using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFDoc : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du Client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Client
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Commande, incrémenté automatiquement, unique pour le client
        /// </summary>
        public override long No { get; set; }

        // données

        /// <summary>
        /// la date est fixée quand le bon de commande est envoyé.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// No de la Livraison pour une Commande, de la Facture pour une Livraison.
        /// </summary>
        public long? NoGroupe { get; set; }

        /// <summary>
        /// Date de la Livraison pour une Commande, de la Facture pour une Livraison.
        /// </summary>
        public DateTime? DateGroupe { get; set; }

        /// <summary>
        /// 'C' ou 'L' ou 'F'.
        /// Présent uniquement si le document fait partie d'une liste de vues.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Absent quand le document fait partie d'une liste.
        /// Présent quand le document fait partie d'une synthèse si certaines des lignes concernent des produits qui ont changé
        /// par rapport au catalogue en vigueur et contient seulement l'ancien état des produits changés.
        /// Présent si le document est envoyé pour une vue et contient seulement les produits des lignes du document.
        /// Absent quand le document est reçu.
        /// </summary>
        public Catalogues.Catalogue Tarif { get; set; }

        /// <summary>
        /// Lignes du document
        /// </summary>
        public ICollection<CLFLigneData> Lignes { get; set; }

        /// <summary>
        /// Nombre de lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        public int? NbLignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// Présent et faux si le document contient des lignes dont le coût n'est pas calculable.
        /// Fixé quand le document est enregistré
        /// </summary>
        public bool? Incomplet { get; set; }

    }
}
