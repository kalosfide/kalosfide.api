using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class DocCLF: AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du Client
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Client
        /// </summary>
        public override int Rno { get; set; }

        /// <summary>
        /// No du document, incrémenté automatiquement par client pour une commande, par site pour une livraison ou une facture
        /// </summary>
        public override long No { get; set; }

        /// <summary>
        /// 'C' ou 'L' ou 'F'.
        /// </summary>
        public string Type { get; set; }

        // données

        /// <summary>
        /// la date est fixée quand le bon de commande est envoyé.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// No de la Livraison pour une Commande, de la Facture pour une Livraison.
        /// </summary>
        public long? NoGroupe { get; set; }

        // Le Site sert à indexer
        // Uid du site
        public string SiteUid { get; set; }
        // Rno du site
        public int SiteRno { get; set; }

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

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<DocCLF> entité = builder.Entity<DocCLF>();

            entité.Property(ligne => ligne.Uid).IsRequired();
            entité.Property(ligne => ligne.Uid).HasMaxLength(LongueurMax.UId);
            entité.Property(ligne => ligne.Rno).IsRequired();
            entité.Property(ligne => ligne.No).IsRequired();
            entité.Property(ligne => ligne.Type).IsRequired();
            entité.Property(ligne => ligne.SiteUid).IsRequired();
            entité.Property(ligne => ligne.SiteUid).HasMaxLength(LongueurMax.UId);
            entité.Property(ligne => ligne.SiteRno).IsRequired();
            entité.Property(ligne => ligne.Total).HasColumnType(PrixProduitDef.Type);

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Rno,
                donnée.No,
                donnée.Type
            });

            entité.ToTable("Docs");
        }
    }
}
