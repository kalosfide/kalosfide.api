using System;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveClient : AvecIdUint, IAvecDate, IClientDataAnnullable, IAvecIdUintEtDateEtEtatAnnulable
    {
        [Required]
        public DateTime Date { get; set; }

        [StringLength(1)]
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
        virtual public Client Client { get; set; }

        // utile
        /// <summary>
        /// Crée une copie avec une autre clé de client.
        /// </summary>
        public static ArchiveClient Clone(uint id, ArchiveClient archive)
        {
            ArchiveClient clone = new ArchiveClient
            {
                Id = id,
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
            var entité = builder.Entity<ArchiveClient>();

            entité.HasKey(donnée => new { donnée.Id, donnée.Date });

            entité.HasIndex(donnée => donnée.Id);

            entité
                .HasOne(a => a.Client)
                .WithMany(c => c.Archives)
                .HasForeignKey(a => a.Id)
                .HasPrincipalKey(c => c.Id);

            entité.ToTable("ArchiveClient");
        }
    }
}
