using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
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

    public class DemandeSiteDate : AvecIdUint
    {
        /// <summary>
        /// Date de la demande.
        /// </summary>
        public DateTime Date { get; set; }

    }

    public class DemandeSiteEnvoi
    {
        /// <summary>
        /// Date d'envoi du message d'activation.
        /// </summary>
        public DateTime? Envoi { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DemandeSiteVueSansDates : IFournisseurData
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
        /// Url et Titre du Site
        /// </summary>
        [JsonProperty]
        public SiteAAjouter Site { get; set; }

        /// <summary>
        /// Email de l'Utilisateur
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        public DemandeSiteVueSansDates(DemandeSite demande)
        {
            Email = demande.Email;
            Site = new SiteAAjouter();
            Fournisseur.CopieData(demande.Fournisseur, this);
            Data.Site.CopieData(demande.Fournisseur.Site, Site);
        }

    }

    public class DemandSiteAActiverData : DemandeSiteVueSansDates
    {
        public bool EstUtilisateur { get; set; }

        public DemandSiteAActiverData(DemandeSite demande, bool estUtilisateur) : base(demande)
        {
            EstUtilisateur = estUtilisateur;
        }
    }

    public class DemandSiteAActiver : ICréeCompteVue
    {
        /// <summary>
        /// Email de l'Utilisateur à créer.
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Mot de passe de l'Utilisateur à créer. Inutile si l'utilisateur a modifié le Fournisseur ou le Site.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Code de validation qui contient la DemandeSite originale.
        /// </summary>
        [Required]
        public string Code { get; set; }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DemandeSiteVue : DemandeSiteVueSansDates
    {
        public uint Id { get; set; }
        /// <summary>
        /// Date de la demande.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Date d'envoi du message d'activation.
        /// </summary>
        public DateTime? Envoi { get; set; }

        public DemandeSiteVue(DemandeSite demande) : base(demande)
        {
            Id = demande.Id;
            Date = demande.Date;
            Envoi = demande.Envoi;
        }
    }
}
