using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class LigneCLFArchiveProduit
    {
        /// <summary>
        /// Uid dde la Ligne
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// Rno dde la Ligne
        /// </summary>
        public int Rno { get; set; }

        /// <summary>
        /// No du document
        /// </summary>
        public long No { get; set; }

        /// <summary>
        /// Uid2 de la ligne et Uid du Produit
        /// </summary>
        public string Uid2 { get; set; }

        /// <summary>
        /// Rno2 de la ligne et Rno du Produit
        /// </summary>
        public int Rno2 { get; set; }

        /// <summary>
        /// No2 de la ligne et No du Produit
        /// </summary>
        public long No2 { get; set; }

        /// <summary>
        /// Date de l'archive Produit
        /// </summary>
        public DateTime Date { get; set; }

        // navigation
        public virtual LigneCLF Ligne { get; set; }
        public virtual ArchiveProduit Produit { get; set; }

        // création
        public static void LieTable(ModelBuilder builder)
        {
            var entité = builder.Entity<LigneCLFArchiveProduit>();

            entité.HasKey(lap => new {
                lap.Uid,
                lap.Rno,
                lap.No,
                lap.Uid2,
                lap.Rno2,
                lap.No2,
                lap.Date
            });

            entité.HasOne(apc => apc.Produit).WithMany(ap => ap.LigneArchiveProduits)
                .HasPrincipalKey(ap => new { ap.Uid, ap.Rno, ap.No, ap.Date })
                .HasForeignKey(apc => new { apc.Uid2, apc.Rno2, apc.No2, apc.Date });

            entité.HasOne(lap => lap.Ligne).WithMany(ap => ap.LigneArchiveProduits)
                .HasPrincipalKey(l => new { l.Uid, l.Rno, l.No, l.Uid2, l.Rno2, l.No2 })
                .HasForeignKey(lap => new { lap.Uid, lap.Rno, lap.No, lap.Uid2, lap.Rno2, lap.No2 });

        }
    }
}
