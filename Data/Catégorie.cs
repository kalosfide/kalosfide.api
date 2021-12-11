using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class Catégorie : AKeyUidRnoNo, IAvecDate
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

        /// <summary>
        /// Date de la création jusqu'à fin de la modification de catalogue où cette catégorie est créé.
        /// Date de la fin de la modification de catalogue où cette catégorie a été ajouté ou modifié.
        /// C'est aussi celle de la dernière archive de cette catégorie.
        /// </summary>
        public DateTime Date { get; set; }

        // navigation
        virtual public ICollection<ArchiveCatégorie> Archives { get; set; }
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
