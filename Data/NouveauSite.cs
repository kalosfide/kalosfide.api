using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class NouveauSite: IRoleData, ISiteDef
    {
        // key

        /// <summary>
        /// Email de l'utilisateur
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string Email { get; set; }

        // date
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Nom du Fournisseur
        /// </summary>
        [Required]
        public string Nom { get; set; }

        /// <summary>
        /// Adresse du Fournisseur
        /// </summary>
        [Required]
        public string Adresse { get; set; }

        /// <summary>
        /// Ville du Fournisseur
        /// </summary>
        [Required]
        public string Ville { get; set; }

        /// <summary>
        /// Url du site
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// Titre des pages
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string Titre { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<NouveauSite>();

            entité.HasKey(donnée => new
            {
                donnée.Email
            });

            entité.ToTable("NouveauxSites");
        }
    }
}
