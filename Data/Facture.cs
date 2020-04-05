using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class Facture : AKeyUidRnoNo
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
        /// No de la facture, incrémenté automatiquement, unique pour le site
        /// </summary>
        [Required]
        public override long No { get; set; }

        // pour indexer
        [MaxLength(LongueurMax.UId)]
        public string SiteUid { get; set; }
        public int SiteRno { get; set; }

        // données
        public DateTime? Date { get; set; }

        // navigation
        virtual public Client Client { get; set; }
        virtual public ICollection<Livraison> Livraisons { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Facture>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité.HasIndex(donnée => donnée.Date);

            entité.HasIndex(donnée => new { Uid = donnée.SiteUid, Rno = donnée.SiteRno });

            entité
                .HasOne(facture => facture.Client)
                .WithMany(client => client.Factures)
                .HasForeignKey(facture => new { facture.Uid, facture.Rno })
                .OnDelete(DeleteBehavior.ClientSetNull);

            entité.ToTable("Factures");
        }
    }
}
