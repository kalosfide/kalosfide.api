using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{
    public class LigneLivraison : AKeyUidRnoNo2
    {
        /// <summary>
        /// Uid du Role et du Client du client et de la Livraison
        /// </summary>
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et de la Livraison
        /// </summary>
        [Required]
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Livraison
        /// </summary>
        [Required]
        public override long No { get; set; }

        /// <summary>
        /// Uid du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid2 { get; set; }

        /// <summary>
        /// Rno du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        [Required]
        public override int Rno2 { get; set; }

        /// <summary>
        /// No du Produit
        /// </summary>
        [Required]
        public override long No2 { get; set; }

        // données

        [Column(TypeName = QuantitéDef.Type)]
        public decimal? ALivrer { get; set; }

        /// <summary>
        /// Supprimé quand la livraison a été facturée
        /// </summary>
        [Column(TypeName = QuantitéDef.Type)]
        public decimal? AFacturer { get; set; }

        // navigation
        virtual public Livraison Livraison { get; set; }
        virtual public Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<LigneLivraison> entité = builder.Entity<LigneLivraison>();

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
                .HasOne(dl => dl.Livraison)
                .WithMany(l => l.Détails)
                .HasForeignKey(dl => new { dl.Uid, dl.Rno, dl.No })
                .HasPrincipalKey(l => new { l.Uid, l.Rno, l.No });

            entité
                .HasOne(dl => dl.Produit)
                .WithMany()
                .HasForeignKey(dl => new { dl.Uid2, dl.Rno2, dl.No2 })
                .HasPrincipalKey(p => new { p.Uid, p.Rno, p.No })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("DétailLivraisons");
        }
    }
}