using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveProduit : AvecIdUint, IAvecDate, IProduitDataAnnulable
    {

        /// <summary>
        /// Date de la fin de la modification de catalogue où cette archive a été ajoutée.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        // données
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }
        public uint? CategorieId { get; set; }

        public TypeMesure? TypeMesure { get; set; }
        public TypeCommande? TypeCommande { get; set; }
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal? Prix { get; set; }

        public bool? Disponible { get; set; }

        // navigation
        virtual public Produit Produit { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveProduit>();

            entité.HasKey(donnée => new { donnée.Id, donnée.Date });

            entité.HasIndex(donnée => donnée.Id);

            entité
                .HasOne(archive => archive.Produit)
                .WithMany(produit => produit.Archives)
                .HasForeignKey(archive => archive.Id)
                .HasPrincipalKey(produit => produit.Id);

            entité.ToTable("ArchiveProduits");
        }
    }
}
