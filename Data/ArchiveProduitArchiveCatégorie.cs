using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class ArchiveProduitArchiveCatégorie
    {
        public string UidProduit { get; set; }
        public int RnoProduit { get; set; }
        public long NoProduit { get; set; }
        public DateTime DateProduit { get; set; }
        public string UidCatégorie { get; set; }
        public int RnoCatégorie { get; set; }
        public long NoCatégorie { get; set; }
        public DateTime DateCatégorie { get; set; }

        // navigation
        public virtual ArchiveProduit Produit { get; set; }
        public virtual ArchiveCatégorie Catégorie { get; set; }

        // création
        public static void LieTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveProduitArchiveCatégorie>();

            entité.HasKey(apc => new { apc.UidProduit, apc.RnoProduit, apc.NoProduit, apc.DateProduit,
                apc.UidCatégorie, apc.RnoCatégorie, apc.NoCatégorie, apc.DateCatégorie });

            entité.HasOne(apc => apc.Produit).WithMany(ap => ap.ArchiveProduitCatégories)
                .HasPrincipalKey(ap => new { ap.Uid, ap.Rno, ap.No, ap.Date })
                .HasForeignKey(apc => new { apc.UidProduit, apc.RnoProduit, apc.NoProduit, apc.DateProduit });

            entité.HasOne(apc => apc.Catégorie).WithMany(ac => ac.ArchiveProduitCatégories)
                .HasPrincipalKey(ac => new { ac.Uid, ac.Rno, ac.No, ac.Date })
                .HasForeignKey(apc => new { apc.UidCatégorie, apc.RnoCatégorie, apc.NoCatégorie, apc.DateCatégorie });

        }
    }
}
