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
    public class ClientDIdentifiant : IClientData, IRolePréférences, IRoleEtat
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
            Client.FixeRoleEtat(client, this);
//            Role.CopiePréférences(client, this);
        }

    }
}
