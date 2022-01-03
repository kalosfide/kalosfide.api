using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Produits
{
    public class ProduitAAjouter: IProduitData
    {
        public uint SiteId { get; set; }
        public string Nom { get; set; }
        public uint CategorieId { get; set; }
        public TypeMesure TypeMesure { get; set; }
        public TypeCommande TypeCommande { get; set; }
        public decimal Prix { get; set; }
        public bool Disponible { get; set; }
    }

    public class ProduitAEditer: AvecIdUint, IProduitDataAnnulable
    {
        public string Nom { get; set; }
        public uint? CategorieId { get; set; }
        public TypeMesure? TypeMesure { get; set; }
        public TypeCommande? TypeCommande { get; set; }
        public decimal? Prix { get; set; }
        public bool? Disponible { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données (rendus nullables) et le No d'un Produit.
    /// Objet envoyé dans une liste d'un catalogue.
    /// </summary>
    public class ProduitDeCatalogue: AvecIdUint, IProduitData
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public uint CategorieId { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public TypeCommande TypeCommande { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public TypeMesure TypeMesure { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public decimal Prix { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public bool Disponible { get; set; }

        private ProduitDeCatalogue(uint id)
        {
            Id = id;
        }

        public static ProduitDeCatalogue SansEtatNiDate(Produit produit)
        {
            ProduitDeCatalogue produitDeCatalogue = new ProduitDeCatalogue(produit.Id);
            Produit.CopieData(produit, produitDeCatalogue);
            return produitDeCatalogue;
        }

        public static ProduitDeCatalogue AvecEtat(Produit produit)
        {
            ProduitDeCatalogue produitDeCatalogue = ProduitDeCatalogue.SansEtatNiDate(produit);
            produitDeCatalogue.Disponible = produit.Disponible;
            return produitDeCatalogue;
        }

        /// <summary>
        /// Retrouve l'état d'un produit à une date passée.
        /// </summary>
        /// <param name="archives">archives d'un produit</param>
        /// <param name="date">date d'une fin de modification de catalogue passée</param>
        /// <returns></returns>
        public static ProduitDeCatalogue ALaDate(IEnumerable<ArchiveProduit> archives, DateTime date)
        {
            ArchiveProduit[] archivesAvantDate = archives.Where(a => a.Date <= date).OrderBy(a => a.Date).ToArray();
            ProduitDeCatalogue produitDeCatalogue = new ProduitDeCatalogue(archivesAvantDate.First().Id)
            {
                Date = date
            };
            foreach (ArchiveProduit archive in archivesAvantDate)
            {
                Produit.CopieDataSiPasNull(archive, produitDeCatalogue);
            }
            return produitDeCatalogue;
        }
    }
}
