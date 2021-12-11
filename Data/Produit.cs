using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class Produit: AKeyUidRnoNo, IAvecDate
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public override long No { get; set; }

        // données
        /// <summary>
        /// Nom du produit
        /// modifiable après la création (nécessaire en cas de faute d'orthographe)
        /// </summary>
        [Required]
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }

        /// <summary>
        /// No de la Catégorie du produit
        /// non modifiable après la création
        /// </summary>
        [Required]
        public long CategorieNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [StringLength(1)]
        public string TypeMesure { get; set; }
        [Required]
        [StringLength(1)]
        public string TypeCommande { get; set; }
        [Required]
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal Prix { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }
        /// <summary>
        /// Date de la création jusqu'à fin de la modification de catalogue où ce produit est créé.
        /// Date de la fin de la modification de catalogue où ce produit a été ajouté ou modifié.
        /// C'est aussi celle de la dernière archive de ce produit.
        /// </summary>
        public DateTime Date { get; set; }

        // navigation
        virtual public ICollection<ArchiveProduit> Archives { get; set; }
        virtual public Catégorie Catégorie { get; set; }

        virtual public ICollection<LigneCLF> Lignes { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Produit>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.No });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno, donnée.Nom }).IsUnique();

            entité.Property(donnée => donnée.TypeMesure).HasDefaultValue(UnitéDeMesure.Aucune);
            entité.Property(donnée => donnée.TypeCommande).HasDefaultValue(TypeUnitéDeCommande.Unité);
            entité.Property(donnée => donnée.Prix).HasDefaultValue(0);

            entité.Property(donnée => donnée.Etat).HasDefaultValue(TypeEtatProduit.Disponible);

            entité
                .HasOne(produit => produit.Catégorie)
                .WithMany(catégorie => catégorie.Produits)
                .HasForeignKey(produit => new { produit.Uid, produit.Rno, produit.CategorieNo })
                .HasPrincipalKey(catégorie => new { catégorie.Uid, catégorie.Rno, catégorie.No });

            entité.ToTable("Produits");
        }
    }
}
