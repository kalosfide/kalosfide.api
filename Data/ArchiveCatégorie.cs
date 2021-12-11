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
    public class ArchiveCatégorie: AKeyUidRnoNo, IAvecDate, ICatégorieData
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public override long No { get; set; }
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

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité
                .HasOne(archive => archive.Catégorie)
                .WithMany(catégorie => catégorie.Archives)
                .HasForeignKey(archive => new { archive.Uid, archive.Rno, archive.No })
                .HasPrincipalKey(catégorie => new { catégorie.Uid, catégorie.Rno, catégorie.No });

            entité.ToTable("ArchiveCatégories");
        }

    }
}
