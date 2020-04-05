using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{
    public class ContexteCommande
    {

        /// <summary>
        /// Etat du site
        /// </summary>
        public string EtatSite { get; set; }

        /// <summary>
        /// No de la livraison
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date de la livraison
        /// </summary>
        public DateTime? DateLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }

        public long? NoDC { get; set; } // no de la dernière commande si elle existe
    }
}
