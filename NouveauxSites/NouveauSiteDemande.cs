using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.NouveauxSites
{
    public class NouveauSiteDemande: IRoleData, ISiteDef
    {
        // key

        /// <summary>
        /// Email de l'utilisateur
        /// </summary>
        public string Email { get; set; }

        // date

        /// <summary>
        /// Nom du Fournisseur
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Adresse du Fournisseur
        /// </summary>
        public string Adresse { get; set; }

        /// <summary>
        /// Ville du Fournisseur
        /// </summary>
        public string Ville { get; set; }

        /// <summary>
        /// Url du site
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Titre des pages
        /// </summary>
        public string Titre { get; set; }
    }
}
