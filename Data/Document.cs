using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class Document
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
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        public TypeCLF Type { get; set; }

        // données

        /// <summary>
        /// Date de l'enregistrement du document (envoi pour un bon de commande).
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// No de la Livraison pour une Commande, de la Facture pour une Livraison.
        /// </summary>
        public uint? NoGroupe { get; set; }

        /// <summary>
        /// Nombre de lignes.
        /// </summary>
        public int NbLignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Présent et vrai si le document est un bon de commande qui contient des lignes dont le coût n'est pas calculable.
        /// </summary>
        public bool? Incomplet { get; set; }

        public byte[] Pdf { get; set; }
    }
}
