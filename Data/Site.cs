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

    public interface ISiteData
    {
        string Url { get; set; }
        string Titre { get; set; }
    }

    public interface ISiteDataAnnulable
    {
        string Url { get; set; }
        string Titre { get; set; }
    }

    public class Site : AvecIdUint, ISiteData
    {

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
        public virtual Fournisseur Fournisseur { get; set; }
        public virtual ICollection<Client> Clients { get; set; }

        virtual public ICollection<Produit> Produits { get; set; }
        virtual public ICollection<Catégorie> Catégories { get; set; }

        virtual public ICollection<ArchiveSite> Archives { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Site>();

            entité.HasKey(donnée => new { donnée.Id });

            // quand le site est créé, la modification du catalogue commence 
            entité.Property(donnée => donnée.Ouvert).HasDefaultValue(false);

            entité.HasIndex(donnée => donnée.Url);

            entité.ToTable("Sites");
        }

        // utiles
        public static void CopieData(ISiteData de, ISiteData vers)
        {
            vers.Url = de.Url;
            vers.Titre = de.Titre;
        }

        public static void CopieData(Site de, ISiteDataAnnulable vers)
        {
            vers.Url = de.Url;
            vers.Titre = de.Titre;
        }

        public static void CopieData(ISiteDataAnnulable de, ISiteDataAnnulable vers)
        {
            vers.Url = de.Url;
            vers.Titre = de.Titre;
        }
        public static void CopieDataSiPasNull(ISiteDataAnnulable de, ISiteData vers)
        {
            if (de.Url != null) { vers.Url = de.Url; }
            if (de.Titre != null) { vers.Titre = de.Titre; }
        }
        public static void CopieDataSiPasNullOuComplète(ISiteDataAnnulable de, ISiteData vers, ISiteData pourCompléter)
        {
            vers.Url = de.Url ?? pourCompléter.Url;
            vers.Titre = de.Titre ?? pourCompléter.Titre;
        }

        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(ISiteData ancien, ISiteDataAnnulable nouveau, ISiteDataAnnulable différences)
        {
            bool modifié = false;
            if (nouveau.Url != null && ancien.Url != nouveau.Url)
            {
                différences.Url = nouveau.Url;
                ancien.Url = nouveau.Url;
                modifié = true;
            }
            if (nouveau.Titre != null && ancien.Titre != nouveau.Titre)
            {
                différences.Titre = nouveau.Titre;
                ancien.Titre = nouveau.Titre;
                modifié = true;
            }
            return modifié;
        }

        /// <summary>
        /// Vérifie que Url et Titre sont présents et non vides.
        /// </summary>
        /// <param name="siteDef"></param>
        /// <param name="modelState"></param>
        public static void VérifieTrim(ISiteData siteDef, ModelStateDictionary modelState)
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

        public static string[] AvérifierSansEspacesData
        {
            get
            {
                return new string[]
                {
                    nameof(Site.Url),
                    nameof(Site.Titre)
                };
            }
        }

        public static string[] AvérifierSansEspacesDataAnnulable
        {
            get
            {
                return new string[]
                {
                    nameof(Site.Url),
                    nameof(Site.Titre)
                };
            }
        }
    }
}
