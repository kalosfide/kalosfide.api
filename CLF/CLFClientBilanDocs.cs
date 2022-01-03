using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFBilanDocs
    {
        /// <summary>
        /// Type CLF des documents.
        /// </summary>
        public TypeCLF Type { get; set; }

        /// <summary>
        /// Nombre des documents.
        /// </summary>
        public int Nb { get; set; }

        /// <summary>
        /// Coût total des documents.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Présent et vrai si un des documents contient des lignes dont le coût n'est pas calculable.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Incomplet { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]

    public class CLFClientBilanDocs
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        public List<CLFBilanDocs> Bilans { get; set; }
    }
}
