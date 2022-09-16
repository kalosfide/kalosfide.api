using KalosfideAPI.Data.Keys;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KalosfideAPI.Data.Constantes;

namespace KalosfideAPI.Data
{
    public class LigneCLF: IKeyLigneSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public uint No { get; set; }

        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        /// <summary>
        /// Date du Produit de la ligne au moment de l'enregistrement de son DocCLF.
        /// Quand une ligne est ajoutée à un bon, elle a la date du catalogue au moment de l'ajout.
        /// Quand un bon de commande est envoyé, ses lignes prennent la date du catalogue au moment de l'envoi.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons avec la date qu'ils ont déjà.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        public TypeCLF Type { get; set; }

        // données

        /// <summary>
        /// Quantité du produit
        /// </summary>
        public decimal? Quantité { get; set; }

        /// <summary>
        /// Quantité du produit à fixer pour le document de synthèse parent du document de la ligne.
        /// Supprimé quand le document de synthèse a été envoyé.
        /// </summary>
        public decimal? AFixer { get; set; }

        // navigation
        virtual public DocCLF Doc { get; set; }
        virtual public Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<LigneCLF> entité = builder.Entity<LigneCLF>();

            entité.Property(ligne => ligne.Id).IsRequired();
            entité.Property(ligne => ligne.No).IsRequired();
            entité.Property(ligne => ligne.ProduitId).IsRequired();
            entité.Property(ligne => ligne.Date).IsRequired();
            entité.Property(ligne => ligne.Type).IsRequired();

            entité.HasKey(ligne => new
            {
                ligne.Id,
                ligne.No,
                ligne.ProduitId,
                ligne.Date,
                ligne.Type
            });

            entité.Property(ligne => ligne.Quantité).HasColumnType(QuantitéDef.Type);
            entité.Property(ligne => ligne.AFixer).HasColumnType(QuantitéDef.Type);

            entité
                .HasOne(ligne => ligne.Doc)
                .WithMany(doc => doc.Lignes)
                .HasForeignKey(ligne => new { ligne.Id, ligne.No, ligne.Type })
                .HasPrincipalKey(doc => new { doc.Id, doc.No, doc.Type });

            entité
                .HasOne(ligne => ligne.Produit)
                .WithMany(produit => produit.Lignes)
                .HasForeignKey(ligne => new { ligne.ProduitId })
                .HasPrincipalKey(produit => new { produit.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("Lignes");
        }

        /// <summary>
        /// Crée une copie avec une autre clé de client.
        /// </summary>
        public static LigneCLF Clone(uint id, LigneCLF ligne)
        {
            LigneCLF copie = new LigneCLF
            {
                Id = ligne.Id,
                No = ligne.No,
                ProduitId = ligne.ProduitId,
                Type = ligne.Type,
                Date = ligne.Date,
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer
            };
            return copie;
        }

        /// <summary>
        /// Crée une copie avec une autre date.
        /// </summary>
        public static LigneCLF Clone(DateTime date, LigneCLF ligne)
        {
            LigneCLF copie = new LigneCLF
            {
                Id = ligne.Id,
                No = ligne.No,
                ProduitId = ligne.ProduitId,
                Type = ligne.Type,
                Date = date,
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer
            };
            return copie;
        }
    }
}
