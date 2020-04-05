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
        [MaxLength(Constantes.LongueurMax.UId)]
        public string Uid { get; set; }
        [Required]
        public DateTime Date { get; set; }

        // données
        [StringLength(1)]
        public string Etat { get; set; }

        /// <summary>
        /// à vérifier et mettre à jour à chaque action de l'utilisateur
        /// </summary>
        public int? NoDernierRole { get; set; }

        // navigation
        virtual public Utilisateur Utilisateur { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveUtilisateur>();

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Date
            });

            entité.HasIndex(donnée => donnée.Uid);

            entité
                .HasOne(eu => eu.Utilisateur)
                .WithMany(u => u.Etats)
                .HasForeignKey(eu => eu.Uid)
                .HasPrincipalKey(u => u.Uid);

            entité.ToTable("ArchiveUtilisateurs");
        }
    }
}
