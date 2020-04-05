using KalosfideAPI.Catalogues;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Documents
{

    public class DocumentCommande : AKeyUidRnoNo
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
        /// No de la Commande
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Date de la Commande
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// No de la Livraison
        /// </summary>
        public long? LivraisonNo { get; set; }

        /// <summary>
        /// Date de la Livraison
        /// </summary>
        public DateTime? DateLivraison { get; set; }

        public long? FactureNo { get; set; }

        /// <summary>
        /// Présent si la commande est chargée pour le document.
        /// </summary>
        public List<DétailCommandes.DétailCommandeData> Détails { get; set; }

        /// <summary>
        ///  Nombre de détails avec demande. Présent si la commande est chargée pour la liste.
        /// </summary>
        public int? LignesC { get; set; }

        /// <summary>
        /// Coût total des détails avec demande. Présent si la commande est chargée pour la liste.
        /// </summary>
        public decimal? TotalC { get; set; }

        /// <summary>
        /// Présent et faux si la commande est chargée pour la liste et qu'elle contient des détails dont le coût de la demande n'est pas calculable.
        /// </summary>
        public bool? IncompletC { get; set; }

        /// <summary>
        ///  Nombre de détails avec ALivrer. Présent si la commande est chargée pour la liste.
        /// </summary>
        public int? LignesL { get; set; }

        /// <summary>
        /// Coût total des détails avec ALivrer. Présent si la commande est chargée pour la liste.
        /// </summary>
        public decimal? TotalL { get; set; }

        /// <summary>
        /// Catalogue en vigueur à la date du document. Présent si la commande est chargée pour le document.
        /// </summary>
        public Catalogue Tarif { get; set; }

    }
}
