using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveProduit : AKeyUidRnoNo, IAvecDate, IProduitData
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public override long No { get; set; }
        /// <summary>
        /// Date de la fin de la modification de catalogue où cette archive a été ajoutée.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        // données
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }
        public long? CategorieNo { get; set; }

        [StringLength(1)]
        public string TypeMesure { get; set; }
        [StringLength(1)]
        public string TypeCommande { get; set; }
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal? Prix { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

        // navigation
        virtual public Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveProduit>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité
                .HasOne(archive => archive.Produit)
                .WithMany(produit => produit.Archives)
                .HasForeignKey(archive => new { archive.Uid, archive.Rno, archive.No })
                .HasPrincipalKey(produit => new { produit.Uid, produit.Rno, produit.No });

            entité.ToTable("ArchiveProduits");
        }
    }
}
