using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CLFDocs
    {
        [JsonProperty]
        public List<CLFDoc> ApiDocs { get; set; }

        /// <summary>
        /// Tarif à apliquer à certaines lignes.
        /// Présent si
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Catalogue Tarif { get; set; }

        /// <summary>
        /// Client des documents.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RoleData Client { get; set; }

    }
}
