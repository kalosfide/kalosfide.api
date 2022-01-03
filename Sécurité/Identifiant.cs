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
    public class FournisseurDIdentifiant : IFournisseurData
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        [JsonProperty]
        public string Siret { get; set; }
        [JsonProperty]
        public EtatRole Etat { get; set; }
        [JsonProperty]
        public DateTime Date0 { get; set; }
        [JsonProperty]
        public DateTime DateEtat { get; set; }

        public FournisseurDIdentifiant(Fournisseur fournisseur)
        {
            Etat = fournisseur.Etat;
            IOrderedEnumerable<ArchiveFournisseur> archives = fournisseur.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            Date0 = archives.First().Date;
            DateEtat = archives.Last().Date;
            Fournisseur.CopieData(fournisseur, this);
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
    public class ClientDIdentifiant : IClientData, IRolePréférences
    {
        [JsonProperty]
        public uint Id { get; set; }
        [JsonProperty]
        public EtatRole Etat { get; set; }
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

        public ClientDIdentifiant(Client client)
        {
            Id = client.Id;
            Etat = client.Etat;
            IOrderedEnumerable<ArchiveClient> archives = client.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            Date0 = archives.First().Date;
            DateEtat = archives.Last().Date;
            Client.CopieData(client, this);
//            Role.CopiePréférences(client, this);
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SiteDIdentifiant: ISiteData
    {
        [JsonProperty]
        public uint Id { get; set; }
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
        /// Fournisseur du site
        /// </summary>
        public FournisseurDIdentifiant Fournisseur { get; set; }

        /// <summary>
        /// Client du site pour un role de client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ClientDIdentifiant Client { get; set; }

        /// <summary>
        /// Nombre de documents arrivés depuis la dernière déconnection.
        /// Null si l'utilisateur ne s'est pas déconnecté.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CLFDoc> NouveauxDocs { get; set; }

        private void Copie(Site site)
        {
            Id = site.Id;
            Ouvert = site.Ouvert;
            DateCatalogue = site.DateCatalogue;
            Site.CopieData(site, this);

        }

        public SiteDIdentifiant(Client client)
        {
            Copie(client.Site);
            Fournisseur = new FournisseurDIdentifiant(client.Site.Fournisseur);
            Client = new ClientDIdentifiant(client);
        }

        public SiteDIdentifiant(Fournisseur fournisseur)
        {
            Copie(fournisseur.Site);
            Fournisseur = new FournisseurDIdentifiant(fournisseur);
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
        public EtatUtilisateur Etat { get; set; }
        [JsonProperty]
        public int SessionId { get; set; }

        [JsonProperty]
        public uint IdDernierSite { get; set; }

        [JsonProperty]
        public List<SiteDIdentifiant> Sites { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DemandeSite NouveauSite { get; set; }

        /// <summary>
        /// Crée un identifiant avec une liste de Sites vide.
        /// </summary>
        /// <param name="utilisateur"></param>
        public Identifiant(Utilisateur utilisateur)
        {
            UserId = utilisateur.Id;
            Email = utilisateur.Email;
            Etat = utilisateur.Etat;
            SessionId = utilisateur.SessionId;
            Sites = new List<SiteDIdentifiant>();
        }
    }
}
