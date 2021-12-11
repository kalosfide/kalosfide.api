using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{
    public interface ISiteDef
    {
        string Url { get; set; }
        string Titre { get; set; }
    }

    public class Site : AKeyUidRno, ISiteDef
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
        public string Url { get; set; }

        /// <summary>
        /// Titre des pages
        /// </summary>
        [MaxLength(200)]
        public string Titre { get; set; }

        /// <summary>
        /// Vrai quand il n'y a pas de modification du catalogue en cours.
        /// </summary>
        public bool Ouvert { get; set; }

        /// <summary>
        /// null tant que la première modification du catalogue (qui crée le catalogue) n'est pas terminée.
        /// Date de la fin de la dernière modification du catalogue si le site n'est pas d'état Catalogue.
        /// </summary>
        public DateTime? DateCatalogue { get; set; }

        // navigation
        [JsonIgnore]
        virtual public ICollection<Role> Usagers { get; set; }

        virtual public ICollection<Catégorie> Catégories { get; set; }

        virtual public ICollection<ArchiveSite> Archives { get; set; }

        // utiles
        public static void CopieDef(ISiteDef de, ISiteDef vers)
        {
            vers.Url = de.Url;
            vers.Titre = de.Titre;
        }

        /// <summary>
        /// Vérifie que Url et Titre sont présents et non vides.
        /// </summary>
        /// <param name="siteDef"></param>
        /// <param name="modelState"></param>
        public static void VérifieTrim(ISiteDef siteDef, ModelStateDictionary modelState)
        {
            if (siteDef.Url == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "nom", "Absent");
            }
            else
            {
                siteDef.Url = siteDef.Url.Trim();
                if (siteDef.Url.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "nom", "Vide");
                }
            }
            if (siteDef.Titre == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "adresse", "Absent");
            }
            else
            {
                siteDef.Titre = siteDef.Titre.Trim();
                if (siteDef.Titre.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "adresse", "Vide");
                }
            }
        }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Site>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno });

            // quand le site est créé, la modification du catalogue commence 
            entité.Property(donnée => donnée.Ouvert).HasDefaultValue(false);

            entité.HasIndex(donnée => donnée.Url);

            entité.ToTable("Sites");
        }
    }
}
