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
    [JsonObject(MemberSerialization.OptIn)]
    public class ProduitAEnvoyer: AvecIdUint, IProduitDataAnnulable
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public uint? CategorieId { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public TypeCommande? TypeCommande { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public TypeMesure? TypeMesure { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public decimal? Prix { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public bool? Disponible { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public ProduitBilan[] Bilans { get; set; }

        private ProduitAEnvoyer(uint id)
        {
            Id = id;
        }

        public static ProduitAEnvoyer SansEtatNiDate(Produit produit)
        {
            ProduitAEnvoyer produitAEnvoyer = new ProduitAEnvoyer(produit.Id);
            Produit.CopieData(produit, produitAEnvoyer);
            return produitAEnvoyer;
        }

        public static ProduitAEnvoyer AvecEtat(Produit produit)
        {
            ProduitAEnvoyer produitAEnvoyer = ProduitAEnvoyer.SansEtatNiDate(produit);
            produitAEnvoyer.Disponible = produit.Disponible;
            if (produit.Lignes != null)
            {
                produitAEnvoyer.Bilans = produit.Lignes
                    .GroupBy(l => l.Type)
                    .Select(g => g.Aggregate(new ProduitBilan { Type = g.Key, Nb = 0, Quantité = 0 },
                    (ProduitBilan bilan, LigneCLF ligne) =>
                    {
                        bilan.Nb++;
                        if (ligne.Quantité != null)
                        {
                            bilan.Quantité += ligne.Quantité.Value;
                        }
                        return bilan;
                    }))
                    .ToArray();
            }
            return produitAEnvoyer;
        }

        /// <summary>
        /// Retrouve l'état d'un produit à une date passée.
        /// </summary>
        /// <param name="archives">archives d'un produit</param>
        /// <param name="date">date d'une fin de modification de catalogue passée</param>
        /// <returns></returns>
        public static ProduitAEnvoyer ALaDate(IEnumerable<ArchiveProduit> archives, DateTime date)
        {
            ArchiveProduit[] archivesAvantDate = archives.Where(a => a.Date <= date).OrderBy(a => a.Date).ToArray();
            ProduitAEnvoyer produitDeCatalogue = new ProduitAEnvoyer(archivesAvantDate.First().Id)
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
