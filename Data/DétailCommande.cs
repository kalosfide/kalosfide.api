using KalosfideAPI.CLF;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{
    public class DétailCommande : AKeyUidRnoNo2
    {
        /// <summary>
        /// Uid du Role et du Client du client et de la Commande
        /// </summary>
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et de la Commande
        /// </summary>
        [Required]
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Commande
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

        /// <summary>
        /// Date de la commande.
        /// Présent si la ligne est dans une livraison ou une facture et le produit a changé de prix.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Indique si Demande est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut de type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        [StringLength(1)]
        public string TypeCommande { get; set; }
        [Column(TypeName = QuantitéDef.Type)]
        public decimal? Demande { get; set; }

        /// <summary>
        /// Supprimé quand la commande a fait l'objet d'un bon de livraison.
        /// </summary>
        [Column(TypeName = QuantitéDef.Type)]
        public decimal? ALivrer { get; set; }

        /// <summary>
        /// A SUPPRIMER
        /// </summary>
        [Column(TypeName = QuantitéDef.Type)]
        public decimal? AFacturer { get; set; }

        // navigation
        virtual public Commande Commande { get; set; }
        virtual public Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<DétailCommande> entité = builder.Entity<DétailCommande>();

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
                .HasOne(dc => dc.Commande)
                .WithMany(c => c.Détails)
                .HasForeignKey(dc => new { dc.Uid, dc.Rno, dc.No })
                .HasPrincipalKey(c => new { c.Uid, c.Rno, c.No });

            entité
                .HasOne(dc => dc.Produit)
                .WithMany()
                .HasForeignKey(dc => new { dc.Uid2, dc.Rno2, dc.No2 })
                .HasPrincipalKey(p => new { p.Uid, p.Rno, p.No })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("DétailCommandes");
        }
    }
}