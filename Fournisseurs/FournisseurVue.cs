using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    public class FournisseurAEditer: AvecIdUint, IFournisseurDataAnnullable
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Siret { get; set; }
        public EtatRole? Etat { get; set; }
    }
    public class FournisseurAAjouter: IFournisseurData
    {
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Adresse { get; set; }
        [Required]
        public string Ville { get; set; }
        [Required]
        public string Siret { get; set; }
        [Required]
        public SiteAAjouter Site { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class FournisseurVue : AvecIdUint, IFournisseurData, IRoleEtat, ISiteData
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        public string Siret { get; set; }

        /// <summary>
        /// Une des valeurs de TypeEtatRole.
        /// </summary>
        [JsonProperty]
        public EtatRole Etat { get; set; }

        /// <summary>
        /// Date de création.
        /// </summary>
        [JsonProperty]
        public DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        [JsonProperty]
        public DateTime DateEtat { get; set; }

        /// <summary>
        /// Url du Site
        /// </summary>
        [JsonProperty]
        public string Url { get; set; }

        /// <summary>
        /// Titre du Site
        /// </summary>
        [JsonProperty]
        public string Titre { get; set; }

        /// <summary>
        /// Email de l'Utilisateur
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        public FournisseurVue(Fournisseur fournisseur)
        {
            Id = fournisseur.Id;
            Fournisseur.CopieData(fournisseur, this);
            Site.CopieData(fournisseur.Site, this);
            Email = fournisseur.Utilisateur.Email;
        }
    }

}
