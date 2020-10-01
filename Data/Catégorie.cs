using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KalosfideAPI.Data.Constantes;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class Catégorie : Keys.AKeyUidRnoNo
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public override long No { get; set; }

        // données
        [Required]
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }

        // navigation
        virtual public ICollection<ArchiveCatégorie> ArchiveCatégories { get; set; }
        virtual public Site Site { get; set; }
        virtual public ICollection<Produit> Produits { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Catégorie>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno, donnée.Nom }).IsUnique();

            entité
                .HasOne(catégorie => catégorie.Site)
                .WithMany(site => site.Catégories)
                .HasForeignKey(catégorie => new { catégorie.Uid, catégorie.Rno });

            entité.ToTable("Catégories");
        }
    }
}
