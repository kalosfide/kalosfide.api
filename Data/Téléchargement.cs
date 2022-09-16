using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class Téléchargement
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

        /// <summary>
        /// Date du téléchargement du document.
        /// </summary>
        public DateTime Date { get; set; }

        public bool ParLeClient { get; set; }

        public virtual DocCLF DocCLF { get; set; }
        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<Téléchargement> entité = builder.Entity<Téléchargement>();


            entité.HasKey(donnée => new
            {
                donnée.Id,
                donnée.No,
                donnée.Type,
                donnée.Date,
                donnée.ParLeClient
            });

            entité.HasOne(téléchargement => téléchargement.DocCLF).WithMany(docCLF => docCLF.Téléchargements)
                .HasPrincipalKey(docCLF => new { docCLF.Id, docCLF.No, docCLF.Type })
                .HasForeignKey(téléchargement => new { téléchargement.Id, téléchargement.No, téléchargement.Type });

            entité.ToTable("Téléchargements");
        }

    }
}
