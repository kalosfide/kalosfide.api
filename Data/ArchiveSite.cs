using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{
    public class ArchiveSite : AvecIdUint, ISiteDataAnnulable, IAvecDate
    {

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
                donnée.Id,
                donnée.Date
            });

            entité.HasIndex(donnée => donnée.Id);

            entité
                .HasOne(a => a.Site)
                .WithMany(s => s.Archives)
                .HasForeignKey(a => a.Id)
                .HasPrincipalKey(s => s.Id);

            entité.ToTable("ArchiveSites");
        }
    }
}

