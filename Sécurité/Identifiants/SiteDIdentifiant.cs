using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Sécurité.Identifiants
{

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
        /// Liste des documents arrivés depuis la dernière déconnection.
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
}
