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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint IdDernierSite { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Déconnection { get; set; }

        [JsonProperty]
        public List<SiteDIdentifiant> Sites { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<DemandeSiteDIdentifiant> DemandesSite { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<InvitationDIdentifiant> Invitations { get; set; }

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
