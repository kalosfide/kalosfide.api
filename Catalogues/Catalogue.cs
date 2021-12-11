using KalosfideAPI.Catégories;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Catalogues
{
    public class Catalogue : AKeyUidRno
    {
        /// <summary>
        /// Uid du site
        /// </summary>
        [JsonProperty]
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du site
        /// </summary>
        [JsonProperty]
        public override int Rno { get; set; }

        /// <summary>
        /// Date du Catalogue n'existe pas si la modification est en cours
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Données des produits du site à la date du catalogue
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ProduitDeCatalogue> Produits { get; set; }

        /// <summary>
        /// Catégories du site à la date du catalogue
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CatégorieDeCatalogue> Catégories { get; set; }

    }

    public class CatalogueTarif
    {
        public List<ProduitDataSansEtat> Produits { get; set; }
        public List<CatégorieData> Catégories { get; set; }
    }


}
