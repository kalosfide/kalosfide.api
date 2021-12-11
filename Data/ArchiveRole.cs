using System;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveRole : AKeyUidRno, IAvecDate, IRoleData
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        [MaxLength(200)]
        public string Nom { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        [MaxLength(500)]
        public string Adresse { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierCommande { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierLivraison { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierFacture { get; set; }

        // navigation
        virtual public Role Role { get; set; }

        // utile
        /// <summary>
        /// Crée une copie avec une autre clé de client.
        /// </summary>
        public static ArchiveRole Clone(string uid, int rno, ArchiveRole archive)
        {
            ArchiveRole clone = new ArchiveRole
            {
                Uid = uid,
                Rno = rno,
                Date = archive.Date,
                Etat = archive.Etat,
                Nom = archive.Nom,
                Adresse = archive.Adresse,
                Ville = archive.Ville,
                FormatNomFichierCommande = archive.FormatNomFichierCommande,
                FormatNomFichierLivraison = archive.FormatNomFichierLivraison,
                FormatNomFichierFacture = archive.FormatNomFichierFacture
            };
            return clone;
        }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveRole>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno });

            entité
                .HasOne(a => a.Role)
                .WithMany(r => r.Archives)
                .HasForeignKey(a => new { a.Uid, a.Rno })
                .HasPrincipalKey(r => new { r.Uid, r.Rno });

            entité.ToTable("ArchiveRoles");
        }
    }
}
