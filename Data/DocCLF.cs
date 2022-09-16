using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Data
{
    public enum TypeCLF
    {
        Commande = 1,
        Livraison,
        Facture
    }

    public class DocCLF : IKeyDocSansType
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
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        public TypeCLF Type { get; set; }

        // données

        /// <summary>
        /// Date de l'enregistrement du document (envoi pour un bon de commande).
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// No de la Livraison pour une Commande, de la Facture pour une Livraison.
        /// </summary>
        public uint? NoGroupe { get; set; }

        /// <summary>
        /// Nombre de lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        public int? NbLignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// Présent et faux si le document contient des lignes dont le coût n'est pas calculable.
        /// Fixé quand le document est enregistré
        /// </summary>
        public bool? Incomplet { get; set; }

        // navigation

        /// <summary>
        /// Lignes du document
        /// </summary>
        virtual public ICollection<LigneCLF> Lignes { get; set; }

        virtual public Client Client { get; set; }

        virtual public ICollection<Téléchargement> Téléchargements { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<DocCLF> entité = builder.Entity<DocCLF>();

            entité.Property(doc => doc.Total).HasColumnType(PrixProduitDef.Type);

            entité.HasKey(donnée => new
            {
                donnée.Id,
                donnée.No,
                donnée.Type
            });

            entité.ToTable("Docs");
        }

        // utile

        public static TypeCLF TypeBon(TypeCLF typeSynthèse)
        {
            return typeSynthèse switch
            {
                TypeCLF.Livraison => TypeCLF.Commande,
                TypeCLF.Facture => TypeCLF.Livraison,
                _ => throw new ArgumentOutOfRangeException(nameof(typeSynthèse)),
            };
        }

        public static TypeCLF TypeSynthèse(TypeCLF typeBon)
        {
            return typeBon switch
            {
                TypeCLF.Commande => TypeCLF.Livraison,
                TypeCLF.Livraison => TypeCLF.Facture,
                _ => throw new ArgumentOutOfRangeException(nameof(typeBon)),
            };
        }

        /// <summary>
        /// Crée une copie avec une autre clé de client.
        /// </summary>
        public static DocCLF Clone(uint id, DocCLF doc)
        {
            DocCLF copie = new DocCLF
            {
                Id = id,
                No = doc.No,
                Type = doc.Type,
                NbLignes = doc.NbLignes,
                Total = doc.Total
            };
            return copie;
        }
    }
}
