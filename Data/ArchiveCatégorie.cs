using KalosfideAPI.Catégories;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class ArchiveCatégorie: AvecIdUint, IAvecDate, ICatégorieDataAnnulable
    {

        [Required]
        public DateTime Date { get; set; }

        // données
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }

        // navigation
        virtual public Catégorie Catégorie { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveCatégorie>();

            entité.HasKey(donnée => new { donnée.Id, donnée.Date });

            entité.HasIndex(donnée => donnée.Id);

            entité
                .HasOne(archive => archive.Catégorie)
                .WithMany(catégorie => catégorie.Archives)
                .HasForeignKey(archive => archive.Id)
                .HasPrincipalKey(catégorie => catégorie.Id);

            entité.ToTable("ArchiveCatégories");
        }

    }
}
