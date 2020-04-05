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
    public class Document : AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du client
        /// </summary>
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du client
        /// </summary>
        [Required]
        public override int Rno { get; set; }

        /// <summary>
        /// No de la commande ou livraison ou facture
        /// </summary>
        [Required]
        public override long No { get; set; }

        // key du Fournisseur (ou du Site)
        [Required]
        [MaxLength(LongueurMax.UId)]
        public string SiteUid { get; set; }
        [Required]
        public int SiteRno { get; set; }

        // données

        /// <summary>
        /// Indique si le document est un bon de commande, un bon de livraison ou une facture.
        /// La valeur est une constante de TypeDocument.
        /// </summary>
        [StringLength(1)]
        public string Type { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        ///  Nombre de lignes.
        /// </summary>
        public int Lignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// </summary>
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal Total { get; set; }

        /// <summary>
        /// Présent et faux si le document contient des lignes dont le coût n'est pas calculable.
        /// </summary>
        public bool? Incomplet { get; set; }

        // navigation
        virtual public Client Client { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Document>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No, donnée.SiteUid, donnée.SiteRno, donnée.Type });

            entité.HasIndex(donnée => donnée.Date);

            entité
                .HasOne(document => document.Client)
                .WithMany(client => client.Documents)
                .HasForeignKey(document => new { document.Uid, document.Rno })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("Documents");
        }
    }
}
