using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    public class DemandSiteAActiver : AvecIdUint, ICréeCompteVue
    {
        /// <summary>
        /// Présent uniquement si l'utilisateur a modifié les valeurs enregistrées avec la DemandeSite
        /// </summary>
        public FournisseurAEditer Fournisseur { get; set; }
        /// <summary>
        /// Présent uniquement si l'utilisateur a modifié les valeurs enregistrées avec la DemandeSite
        /// </summary>
        public SiteAEditer Site { get; set; }

        /// <summary>
        /// Email de l'Utilisateur à créer.
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Mot de passe de l'Utilisateur à créer. Inutile si l'utilisateur a modifié le Fournisseur ou le Site.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Code de validation qui contient la DemandeSite originale.
        /// </summary>
        [Required]
        public string Code { get; set; }

    }
}
