using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité.Identifiants
{
    public class BilanCatalogue
    {
        /// <summary>
        /// Nombre de produits dans le catalogue d'un site
        /// </summary>
        [JsonProperty]
        public int Produits { get; set; }
        /// <summary>
        /// Nombre de produits disponibles dans le catalogue d'un site
        /// </summary>
        [JsonProperty]
        public int Disponibles { get; set; }
        /// <summary>
        /// Nombre de catégories dans le catalogue d'un site
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
}
