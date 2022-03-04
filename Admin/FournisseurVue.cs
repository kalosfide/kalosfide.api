using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{

    [JsonObject(MemberSerialization.OptIn)]
    public class FournisseurVue : AvecIdUint, IFournisseurData, IRoleEtat, ISiteData
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        [JsonProperty]
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
            if (fournisseur.Utilisateur != null)
            {
                Email = fournisseur.Utilisateur.Email;
            }
            Fournisseur.FixeRoleEtat(fournisseur, this);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DemandeFournisseurVue : AvecIdUint, IFournisseurData, ISiteData
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        [JsonProperty]
        public string Siret { get; set; }

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

        /// <summary>
        /// Date de la demande.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Date d'envoi du message d'activation.
        /// </summary>
        public DateTime? Envoi { get; set; }

        public DemandeFournisseurVue(DemandeSite demande)
        {
            Id = demande.Id;
            Email = demande.Email;
            Date = demande.Date;
            Envoi = demande.Envoi;
            Fournisseur.CopieData(demande.Fournisseur, this);
            Site.CopieData(demande.Fournisseur.Site, this);
        }
    }

}
