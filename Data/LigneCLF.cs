﻿using KalosfideAPI.Data.Keys;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KalosfideAPI.Data.Constantes;

namespace KalosfideAPI.Data
{
    public class LigneCLF: AKeyUidRnoNo2
    {
        /// <summary>
        /// Uid du Role et du Client du client et de la Commande
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Role et du Client du client et de la Commande
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// Uid du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        public override string Uid2 { get; set; }

        /// <summary>
        /// Rno du Produit et aussi du Site, du Role et du Fournisseur du fournisseur
        /// </summary>
        public override int Rno2 { get; set; }

        /// <summary>
        /// No du Produit
        /// </summary>
        public override long No2 { get; set; }

        /// <summary>
        /// Quand une ligne est ajoutée à un bon, elle a la date du catalogue au moment de l'ajout.
        /// Quand un bon de commande est envoyé, ses lignes prennent la date du catalogue au moment de l'envoi.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons avec la date qu'ils ont déjà.
        /// </summary>
        public override DateTime Date { get; set; }

        /// <summary>
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        public string Type { get; set; }

        // données


        /// <summary>
        /// Présent uniquement si le CLFDoc est une commande.
        /// Indique si Quantité est un compte ou une mesure. Inutile si le Produit a un seul type de commande.
        /// Si absent, la valeur par défaut du type de commande associée au TypeMesure du Produit est utilisée.
        /// </summary>
        public string TypeCommande { get; set; }

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

            entité.Property(ligne => ligne.Uid).IsRequired();
            entité.Property(ligne => ligne.Uid).HasMaxLength(LongueurMax.UId);
            entité.Property(ligne => ligne.Rno).IsRequired();
            entité.Property(ligne => ligne.No).IsRequired();
            entité.Property(ligne => ligne.Uid2).IsRequired();
            entité.Property(ligne => ligne.Uid2).HasMaxLength(LongueurMax.UId);
            entité.Property(ligne => ligne.Rno2).IsRequired();
            entité.Property(ligne => ligne.No2).IsRequired();
            entité.Property(ligne => ligne.Date).IsRequired();
            entité.Property(ligne => ligne.Type).IsRequired();

            entité.HasKey(ligne => new
            {
                ligne.Uid,
                ligne.Rno,
                ligne.No,
                ligne.Uid2,
                ligne.Rno2,
                ligne.No2,
                ligne.Date,
                ligne.Type
            });

            entité.Property(ligne => ligne.TypeCommande).HasMaxLength(1);
            entité.Property(ligne => ligne.Quantité).HasColumnType(QuantitéDef.Type);
            entité.Property(ligne => ligne.AFixer).HasColumnType(QuantitéDef.Type);

            entité
                .HasOne(ligne => ligne.Doc)
                .WithMany(doc => doc.Lignes)
                .HasForeignKey(ligne => new { ligne.Uid, ligne.Rno, ligne.No, ligne.Type })
                .HasPrincipalKey(doc => new { doc.Uid, doc.Rno, doc.No, doc.Type });

            entité
                .HasOne(ligne => ligne.Produit)
                .WithMany(produit => produit.Lignes)
                .HasForeignKey(ligne => new { ligne.Uid2, ligne.Rno2, ligne.No2 })
                .HasPrincipalKey(produit => new { produit.Uid, produit.Rno, produit.No })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("Lignes");
        }

        /// <summary>
        /// Crée une copie avec une autre clé de client.
        /// </summary>
        public static LigneCLF Clone(string uid, int rno, LigneCLF ligne)
        {
            LigneCLF copie = new LigneCLF
            {
                Uid = uid,
                Rno = rno,
                No = ligne.No,
                Uid2 = ligne.Uid2,
                Rno2 = ligne.Rno2,
                No2 = ligne.No2,
                Type = ligne.Type,
                Date = ligne.Date,
                TypeCommande = ligne.TypeCommande,
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
                Uid = ligne.Uid,
                Rno = ligne.Rno,
                No = ligne.No,
                Uid2 = ligne.Uid2,
                Rno2 = ligne.Rno2,
                No2 = ligne.No2,
                Type = ligne.Type,
                Date = date,
                TypeCommande = ligne.TypeCommande,
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer
            };
            return copie;
        }
    }
}
