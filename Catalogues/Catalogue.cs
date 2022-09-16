using KalosfideAPI.Catégories;
using KalosfideAPI.Produits;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Catalogues
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Catalogue
    {
        /// <summary>
        /// Date du Catalogue n'existe pas si la modification est en cours
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Données des produits du site à la date du catalogue
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ProduitAEnvoyer> Produits { get; set; }

        /// <summary>
        /// Catégories du site à la date du catalogue
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CatégorieAEnvoyer> Catégories { get; set; }

    }


}
