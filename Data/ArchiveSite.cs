using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{
    public class ArchiveSite : AKeyUidRno, IAvecDate
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
        public string Url { get; set; }
        [MaxLength(200)]
        public string Titre { get; set; }

        /// <summary>
        /// Vrai quand il n'y a pas de modification du catalogue en cours.
        /// </summary>
        public bool? Ouvert { get; set; }

        // navigation
        public virtual Site Site { get; set; }

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

            entité
                .HasOne(a => a.Site)
                .WithMany(s => s.Archives)
                .HasForeignKey(a => new { a.Uid, a.Rno })
                .HasPrincipalKey(s => new { s.Uid, s.Rno });

            entité.ToTable("ArchiveSites");
        }
    }
}

