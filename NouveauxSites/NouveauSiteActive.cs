using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.NouveauxSites
{
    public class NouveauSiteActive: ICréeCompteVue
    {
        /// <summary>
        /// Email de l'utilisateur
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Mot de passe de l'utilisateur
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Code envoyé dans le lien du message email de validation
        /// </summary>
        public string Code { get; set; }
    }
}
