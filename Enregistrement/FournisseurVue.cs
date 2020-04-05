using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Enregistrement
{
    public class EnregistrementFournisseurVue: VueBase
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string NomSite { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierCommande { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierLivraison { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierFacture { get; set; }

    }
}
