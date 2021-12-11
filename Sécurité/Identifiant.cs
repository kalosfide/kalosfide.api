using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Sécurité
{

    [JsonObject(MemberSerialization.OptIn)]
    public class FournisseurDeSiteDeRole : IRoleData
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }

        public FournisseurDeSiteDeRole(Role fournisseur)
        {
            Role.CopieDef(fournisseur, this);
        }
    }
    public class BilanCatalogue
    {
        /// <summary>
        /// Nombre de produits disponibles dans le catalogue d'un site
        /// </summary>
        [JsonProperty]
        public int Produits { get; set; }
        /// <summary>
        /// Nombre de catégories contenant des produits disponibles dans le catalogue d'un site
        /// </summary>
        [JsonProperty]
        public int Catégories { get; set; }
    }
    public class BilanClients
    {
        [JsonProperty]
        public int Nouveaux { get; set; }
        [JsonProperty]
        public int Actifs { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BilanSite
    {
        [JsonProperty]
        public BilanCatalogue Catalogue { get; set; }
        [JsonProperty]
        public BilanClients Clients { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SiteDeRole : AKeyUidRno, ISiteDef
    {
        [JsonProperty]
        public override string Uid { get; set; }
        [JsonProperty]
        public override int Rno { get; set; }
        [JsonProperty]
        public virtual string Url { get; set; }
        [JsonProperty]
        public virtual string Titre { get; set; }
        [JsonProperty]
        public virtual bool Ouvert { get; set; }
        [JsonProperty]
        public virtual DateTime? DateCatalogue { get; set; }

        /// <summary>
        /// Bilan du site pour un role de fournisseur.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BilanSite Bilan { get; set; }

        /// <summary>
        /// Fournisseur du site pour un role de client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FournisseurDeSiteDeRole Fournisseur { get; set; }

        /// <summary>
        /// Nombre de documents arrivés depuis la dernière déconnection.
        /// Null si l'utilisateur ne s'est pas déconnecté.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CLFDoc> NouveauxDocs { get; set; }

        public SiteDeRole(Site site)
        {
            CopieKey(site);
            Ouvert = site.Ouvert;
            DateCatalogue = site.DateCatalogue;
            Site.CopieDef(site, this);
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RoleDIdentifiant : IRoleData, IRolePréférences
    {
        [JsonProperty]
        public int Rno { get; set; }
        [JsonProperty]
        public string Etat { get; set; }
        [JsonProperty]
        public DateTime Date0 { get; set; }
        [JsonProperty]
        public DateTime DateEtat { get; set; }
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        [JsonProperty]
        public string FormatNomFichierCommande { get; set; }
        [JsonProperty]
        public string FormatNomFichierLivraison { get; set; }
        [JsonProperty]
        public string FormatNomFichierFacture { get; set; }
        [JsonProperty]
        public SiteDeRole Site { get; set; }

        public RoleDIdentifiant(Role role)
        {
            Rno = role.Rno;
            Etat = role.Etat;
            var archives = role.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            Date0 = archives.First().Date;
            DateEtat = archives.Last().Date;
            Role.CopieDef(role, this);
            Role.CopiePréférences(role, this);
            Site = new SiteDeRole(role.Site);
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Identifiant
    {
        [JsonProperty]
        public string UserId { get; set; }
        [JsonProperty]
        public string Email { get; set; }

        [JsonProperty]
        public string Uid { get; set; }
        [JsonProperty]
        public string Etat { get; set; }
        [JsonProperty]
        public int SessionId { get; set; }

        [JsonProperty]
        public List<RoleDIdentifiant> Roles { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int NoDernierRole { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public NouveauSite NouveauSite { get; set; }

        /// <summary>
        /// Crée un identifiant avec une liiste de roles vide.
        /// </summary>
        /// <param name="utilisateur"></param>
        public Identifiant(Utilisateur utilisateur)
        {
            UserId = utilisateur.UserId;
            Email = utilisateur.ApplicationUser.Email;
            Uid = utilisateur.Uid;
            Etat = utilisateur.Etat;
            SessionId = utilisateur.SessionId;
            Roles = new List<RoleDIdentifiant>();
        }
    }
}
