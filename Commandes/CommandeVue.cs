using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Commandes
{
    /// <summary>
    /// reproduit l'objet Commande de l'Api en supprimant les élements de clé du détail qui sont ceux de la commande
    /// lecture: contient les données pour afficher une commande comme une ligne
    /// écriture: contient Etat
    /// </summary>
    public class CommandeVue : AKeyUidRnoNo
    {
        public override string Uid { get; set; }    // du client
        public override int Rno { get; set; }       // du client
        public override long No { get; set; }       // du bon de commande

        public long? LivraisonNo { get; set; }

        public DateTime? Date { get; set; }

        public List<DétailCommandes.DétailCommandeData> Details { get; set; }

    }
}
