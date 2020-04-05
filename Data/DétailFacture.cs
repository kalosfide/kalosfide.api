using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class DétailFacture : AKeyUidRnoNo2
    {
        // key de la Facture
        // i.e. key du Fournisseur (ou du Site) + No de la Facture
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public override long No { get; set; }

        // key du Client + No du produit
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid2 { get; set; }
        [Required]
        public override int Rno2 { get; set; }
        [Required]
        public override long No2 { get; set; }

        // données
        [Column(TypeName = QuantitéDef.Type)]
        public decimal ALivrer { get; set; }
        [Column(TypeName = QuantitéDef.Type)]
        public decimal? Servis { get; set; }

        // navigation
        public virtual Facture Facture { get; set; }
        public virtual Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DétailFacture> entité = builder.Entity<DétailFacture>();

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Rno,
                donnée.No,
                donnée.Uid2,
                donnée.Rno2,
                donnée.No2
            });

            entité
                .HasOne(d => d.Facture)
                .WithMany(f => f.Détails)
                .HasForeignKey(d => new { d.Uid, d.Rno, d.No })
                .HasPrincipalKey(f => new { f.Uid, f.Rno, f.No });

            entité
                .HasOne(d => d.Produit)
                .WithMany()
                .HasForeignKey(d => new { d.Uid, d.Rno, d.No2 })
                .HasPrincipalKey(p => new { p.Uid, p.Rno, p.No })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("DétailsFactures");
        }
    }
}