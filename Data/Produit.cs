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

    public enum TypeMesure
    {
        Aucune = 1,
        Kilo,
        Litre
    }

    public enum TypeCommande
    {
        Unité = 1,
        Vrac,
        UnitéOuVrac
    }

    /// <summary>
    /// Contient tous les champs de données hors Date d'un Produit.
    /// </summary>
    public interface IProduitData
    {
        string Nom { get; set; }
        uint CategorieId { get; set; }
        TypeMesure TypeMesure { get; set; }
        TypeCommande TypeCommande { get; set; }
        decimal Prix { get; set; }
        bool Disponible { get; set; }
    }
    /// <summary>
    /// Contient tous les champs rendus nullable hors Date d'un Produit.
    /// </summary>
    public interface IProduitDataAnnulable
    {
        string Nom { get; set; }
        uint? CategorieId { get; set; }
        TypeMesure? TypeMesure { get; set; }
        TypeCommande? TypeCommande { get; set; }
        decimal? Prix { get; set; }
        bool? Disponible { get; set; }
    }

    public class Produit: AvecIdUint, IProduitData, IAvecDate, IAvecSiteId
    {
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
        public uint CategorieId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public TypeMesure TypeMesure { get; set; }
        [Required]
        public TypeCommande TypeCommande { get; set; }
        [Required]
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal Prix { get; set; }

        public bool Disponible { get; set; }
        /// <summary>
        /// Date de la création jusqu'à fin de la modification de catalogue où ce produit est créé.
        /// Date de la fin de la modification de catalogue où ce produit a été ajouté ou modifié.
        /// C'est aussi celle de la dernière archive de ce produit.
        /// </summary>
        public DateTime Date { get; set; }

        // navigation

        public uint SiteId { get; set; }

        public virtual Site Site { get; set; }

        virtual public ICollection<ArchiveProduit> Archives { get; set; }
        virtual public Catégorie Catégorie { get; set; }

        virtual public ICollection<LigneCLF> Lignes { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Produit>();

            entité.HasKey(donnée => donnée.Id);

            entité.HasIndex(donnée => new { donnée.Id, donnée.Nom }).IsUnique();

            entité.Property(donnée => donnée.TypeMesure).HasDefaultValue(TypeMesure.Aucune);
            entité.Property(donnée => donnée.TypeCommande).HasDefaultValue(TypeCommande.Unité);
            entité.Property(donnée => donnée.Prix).HasDefaultValue(0);

            entité
                .HasOne(produit => produit.Catégorie)
                .WithMany(catégorie => catégorie.Produits)
                .HasForeignKey(produit => produit.CategorieId)
                .HasPrincipalKey(catégorie => catégorie.Id);
            entité
                .HasOne(produit => produit.Site)
                .WithMany(site => site.Produits)
                .HasForeignKey(produit => produit.SiteId)
                .HasPrincipalKey(site => site.Id)
                .OnDelete(DeleteBehavior.NoAction);

            entité.ToTable("Produits");
        }

        // utile

        public static void CopieData(IProduitData de, IProduitData vers)
        {
            vers.Nom = de.Nom;
            vers.CategorieId = de.CategorieId;
            vers.TypeCommande = de.TypeCommande;
            vers.TypeMesure = de.TypeMesure;
            vers.Prix = de.Prix;
        }

        public static void CopieData(IProduitData de, IProduitDataAnnulable vers)
        {
            vers.Nom = de.Nom;
            vers.CategorieId = de.CategorieId;
            vers.TypeCommande = de.TypeCommande;
            vers.TypeMesure = de.TypeMesure;
            vers.Prix = de.Prix;
        }
        public static void CopieDataSiPasNull(IProduitDataAnnulable de, IProduitData vers)
        {
            if (de.Nom != null) { vers.Nom = de.Nom; }
            if (de.CategorieId != null) { vers.CategorieId = de.CategorieId.Value; }
            if (de.TypeCommande != null) { vers.TypeCommande = de.TypeCommande.Value; }
            if (de.TypeMesure != null) { vers.TypeMesure = de.TypeMesure.Value; }
            if (de.Prix != null) { vers.Prix = de.Prix.Value; }
            if (de.Disponible != null) { vers.Disponible = de.Disponible.Value; }
        }
        public static void CopieDataSiPasNull(IProduitDataAnnulable de, IProduitDataAnnulable vers)
        {
            if (de.Nom != null) { vers.Nom = de.Nom; }
            if (de.CategorieId != null) { vers.CategorieId = de.CategorieId.Value; }
            if (de.TypeCommande != null) { vers.TypeCommande = de.TypeCommande.Value; }
            if (de.TypeMesure != null) { vers.TypeMesure = de.TypeMesure.Value; }
            if (de.Prix != null) { vers.Prix = de.Prix.Value; }
            if (de.Disponible != null) { vers.Disponible = de.Disponible.Value; }
        }
        public static void CopieDataSiPasNullOuComplète(IProduitDataAnnulable de, IProduitData vers, IProduitData pourCompléter)
        {
            vers.Nom = de.Nom ?? pourCompléter.Nom;
            vers.CategorieId = de.CategorieId ?? pourCompléter.CategorieId;
            vers.TypeCommande = de.TypeCommande ?? pourCompléter.TypeCommande;
            vers.TypeMesure = de.TypeMesure ?? pourCompléter.TypeMesure;
            vers.Prix = de.Prix ?? pourCompléter.Prix;
            vers.Disponible = de.Disponible ?? pourCompléter.Disponible;
        }

        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(IProduitData ancien, IProduitDataAnnulable nouveau, IProduitDataAnnulable différences)
        {
            bool modifié = false;
            if (nouveau.Nom != null && ancien.Nom != nouveau.Nom)
            {
                différences.Nom = nouveau.Nom;
                ancien.Nom = nouveau.Nom;
                modifié = true;
            }
            if (nouveau.CategorieId != null && ancien.CategorieId != nouveau.CategorieId)
            {
                différences.CategorieId = nouveau.CategorieId;
                ancien.CategorieId = nouveau.CategorieId.Value;
                modifié = true;
            }
            if (nouveau.TypeCommande != null && ancien.TypeCommande != nouveau.TypeCommande)
            {
                différences.TypeCommande = nouveau.TypeCommande;
                ancien.TypeCommande = nouveau.TypeCommande.Value;
                modifié = true;
            }
            if (nouveau.TypeMesure != null && ancien.TypeMesure != nouveau.TypeMesure)
            {
                différences.TypeMesure = nouveau.TypeMesure;
                ancien.TypeMesure = nouveau.TypeMesure.Value;
                modifié = true;
            }
            if (nouveau.Prix != null && ancien.Prix != nouveau.Prix)
            {
                différences.Prix = nouveau.Prix;
                ancien.Prix = nouveau.Prix.Value;
                modifié = true;
            }
            if (nouveau.Disponible != null && ancien.Disponible != nouveau.Disponible)
            {
                différences.Disponible = nouveau.Disponible;
                ancien.Disponible = nouveau.Disponible.Value;
                modifié = true;
            }
            return modifié;
        }

        public static string[] AvérifierSansEspacesData
        {
            get
            {
                return new string[] { "Nom" };
            }
        }

        public static string[] AvérifierSansEspacesDataAnnulable
        {
            get
            {
                return new string[] { "Nom" };
            }
        }

        public static bool ValideTypes(TypeMesure typeMesure, TypeCommande typeCommande)
        {
            if (typeCommande == TypeCommande.Unité)
            {
                // un produit qui se commande à l'unité ne doit pas être mesuré
                return typeMesure == TypeMesure.Aucune;
            }
            return typeMesure != TypeMesure.Aucune;
        }

        public static bool ValideTypes(Produit produit)
        {
            return ValideTypes(produit.TypeMesure, produit.TypeCommande);
        }

        public static TypeCommande[] TypesCommandeCompatibles(TypeMesure typeMesure)
        {
            if (typeMesure == TypeMesure.Aucune)
            {
                return new TypeCommande[] { TypeCommande.Unité };
            }
            return new TypeCommande[] { TypeCommande.Vrac, TypeCommande.UnitéOuVrac };
        }

    }
}
