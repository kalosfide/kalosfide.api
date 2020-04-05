using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class Livraison: Keys.AKeyUidRnoNo
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
        /// No de la Livraison, incrémenté automatiquement, unique pour le site
        /// </summary>
        [Required]
        public override long No { get; set; }

        // données
        /// <summary>
        /// Date de la Livraison
        /// </summary>
        public DateTime? Date { get; set; }

        // fixé quand la Livraison est affectée à une Facture
        public long? FactureNo { get; set; }

        // pour indexer
        [MaxLength(LongueurMax.UId)]
        public string SiteUid { get; set; }
        public int SiteRno { get; set; }

        // navigation
        virtual public ICollection<Commande> Commandes { get; set; }
        virtual public ICollection<LigneLivraison> Détails { get; set; }
        virtual public Facture Facture { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Livraison>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité.HasIndex(donnée => donnée.Date);

            entité.HasIndex(donnée => new { Uid = donnée.SiteUid, Rno = donnée.SiteRno });

            entité
                .HasOne(c => c.Facture)
                .WithMany(f => f.Livraisons)
                .HasForeignKey(c => new { c.Uid, c.Rno, c.FactureNo });

            entité.ToTable("Livraisons");
        }
    }
}
