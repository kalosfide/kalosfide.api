using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class ArchiveUtilisateur
    {
        // key
        [Required]
        public string Id { get; set; }
        [Required]
        public DateTime Date { get; set; }

        // données
        public string Email { get; set; }
        public EtatUtilisateur Etat { get; set; }

        /// <summary>
        /// à vérifier et mettre à jour à chaque action de l'utilisateur
        /// </summary>
        public uint? IdDernierSite { get; set; }

        /// <summary>
        /// à mettre à jour à chaque connection et déconection de l'utilisateur
        /// à vérifier à chaque action de l'utilisateur
        /// </summary>
        public int? SessionId { get; set; }

        // navigation
        virtual public Utilisateur Utilisateur { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveUtilisateur>();

            entité.HasKey(donnée => new
            {
                donnée.Id,
                donnée.Date
            });

            entité.HasIndex(donnée => donnée.Id);

            entité
                .HasOne(eu => eu.Utilisateur)
                .WithMany(u => u.Archives)
                .HasForeignKey(eu => eu.Id)
                .HasPrincipalKey(u => u.Id);

            entité.ToTable("ArchiveUtilisateurs");
        }
    }
}
