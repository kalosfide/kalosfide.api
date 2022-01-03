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

    /// <summary>
    /// Contient tous les champs de données hors Date d'une Catégorie.
    /// </summary>
    public interface ICatégorieData
    {
        string Nom { get; set; }
    }
    /// <summary>
    /// Contient tous les champs rendus nullable hors Date d'une Catégorie.
    /// </summary>
    public interface ICatégorieDataAnnulable
    {
        string Nom { get; set; }
    }

    public class Catégorie : AvecIdUint, ICatégorieData, IAvecSiteId
    {

        // données
        [Required]
        [MinLength(10), MaxLength(200)]
        public string Nom { get; set; }

        /// <summary>
        /// Date de la création jusqu'à fin de la modification de catalogue où cette catégorie est créé.
        /// Date de la fin de la modification de catalogue où cette catégorie a été ajouté ou modifié.
        /// C'est aussi celle de la dernière archive de cette catégorie.
        /// </summary>
        public DateTime Date { get; set; }

        // navigation
        virtual public ICollection<ArchiveCatégorie> Archives { get; set; }

        public uint SiteId { get; set; }

        public virtual Site Site { get; set; }

        virtual public ICollection<Produit> Produits { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Catégorie>();

            entité.HasKey(donnée => donnée.Id);

            entité.HasIndex(donnée => new { donnée.Id, donnée.Nom }).IsUnique();

            entité
                .HasOne(catégorie => catégorie.Site)
                .WithMany(site => site.Catégories)
                .HasForeignKey(catégorie => catégorie.SiteId);

            entité.ToTable("Catégories");
        }

        // utile
        public static void CopieData(ICatégorieData de, ICatégorieData vers)
        {
            vers.Nom = de.Nom;
        }
        public static void CopieData(ICatégorieData de, ICatégorieDataAnnulable vers)
        {
            vers.Nom = de.Nom;
        }
        public static void CopieDataSiPasNull(ICatégorieDataAnnulable de, ICatégorieData vers)
        {
            if (de.Nom != null)
            {
                vers.Nom = de.Nom;
            }
        }
        public static void CopieDataSiPasNull(ICatégorieDataAnnulable de, ICatégorieDataAnnulable vers)
        {
            if (de.Nom != null)
            {
                vers.Nom = de.Nom;
            }
        }
        public static void CopieDataSiPasNullOuComplète(ICatégorieDataAnnulable de, ICatégorieData vers, ICatégorieData pourCompléter)
        {
            vers.Nom = de.Nom ?? pourCompléter.Nom;
        }

        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(ICatégorieData ancien, ICatégorieDataAnnulable nouveau, ICatégorieDataAnnulable différences)
        {
            bool modifié = false;
            if (nouveau.Nom != null && ancien.Nom != nouveau.Nom)
            {
                différences.Nom = nouveau.Nom;
                ancien.Nom = nouveau.Nom;
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
    }
}
