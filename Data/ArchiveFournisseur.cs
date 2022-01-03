using System;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveFournisseur : AvecIdUint, IAvecDate, IFournisseurDataAnnullable
    {
        [Required]
        public DateTime Date { get; set; }

        public EtatRole? Etat { get; set; }

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
        public string Siret { get; set; }

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
        virtual public Fournisseur Fournisseur { get; set; }


        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveFournisseur>();

            entité.HasKey(donnée => new { donnée.Id, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Id });

            entité
                .HasOne(a => a.Fournisseur)
                .WithMany(r => r.Archives)
                .HasForeignKey(a => a.Id)
                .HasPrincipalKey(f => f.Id);

            entité.ToTable("ArchiveFournisseur");
        }
    }
}
