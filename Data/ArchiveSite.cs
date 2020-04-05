using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{
    public class ArchiveSite : AKeyUidRno, IKeyArchive
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public DateTime Date { get; set; }

        // données
        [MaxLength(200)]
        public string NomSite { get; set; }
        [MaxLength(200)]
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

        [StringLength(1)]
        public string Etat { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveSite>();

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Rno,
                donnée.Date
            });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno });

            entité.ToTable("ArchiveSites");
        }
    }
}

