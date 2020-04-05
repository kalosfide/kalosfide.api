using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{
    public class Site : AKeyUidRno
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }

        // données

        /// <summary>
        /// Url du site
        /// </summary>
        [MaxLength(200)]
        public string NomSite { get; set; }

        /// <summary>
        /// Titre
        /// </summary>
        [MaxLength(200)]
        public string Titre { get; set; }

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

        [StringLength(1)]
        public string Etat { get; set; }

        // navigation
        [JsonIgnore]
        virtual public ICollection<Role> Usagers { get; set; }

        virtual public ICollection<Produit> Produits { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Site>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno });

            entité.Property(donnée => donnée.Etat).HasDefaultValue(TypeEtatSite.Ouvert);

            entité.ToTable("Sites");
        }
    }
}
