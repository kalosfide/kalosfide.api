using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Produits
{
    /// <summary>
    /// Contient tous les champs de données sauf Etat et Date (rendus nullables) d'un Produit.
    /// </summary>
    public interface IProduitDataSansEtatNiDate
    {
        string Nom { get; set; }
        long? CategorieNo { get; set; }

        string TypeCommande { get; set; }
        string TypeMesure { get; set; }
        decimal? Prix { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données sauf Date (rendus nullables) d'un Produit.
    /// Implémenté par ProduitVue.
    /// </summary>
    public interface IProduitDataSansDate: IProduitDataSansEtatNiDate
    {
        string Etat { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données (rendus nullables) hors Etat et Date d'un Produit.
    /// </summary>
    public interface IProduitDataSansEtat: IProduitDataSansEtatNiDate
    {
        DateTime Date { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données (rendus nullables) d'un Produit.
    /// </summary>
    public interface IProduitData: IProduitDataSansDate
    {
        DateTime Date { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données hors Date (rendus nullables) et la KeyUidRnoNo d'un Produit.
    /// Objet reçu pour ajouter ou éditer un Produit.
    /// </summary>
    public class ProduitVue: AKeyUidRnoNo, IProduitDataSansDate
    {
        // identité
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public override long No { get; set; }

        // données
        public string Nom { get; set; }
        public long? CategorieNo { get; set; }

        public string TypeCommande { get; set; }
        public string TypeMesure { get; set; }
        public decimal? Prix { get; set; }
        public string Etat { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données (rendus nullables) et le No d'un Produit.
    /// Objet envoyé dans une liste d'un catalogue.
    /// </summary>
    public class ProduitDeCatalogue
    {
        [JsonProperty]
        public long No { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public long? CategorieNo { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string TypeCommande { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string TypeMesure { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public decimal? Prix { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Etat { get; set; }

        private ProduitDeCatalogue(long no)
        {
            No = no;
        }

        public static ProduitDeCatalogue SansEtatNiDate(Produit produit)
        {
            ProduitDeCatalogue produitDeCatalogue = new ProduitDeCatalogue(produit.No)
            {
                Nom = produit.Nom,
                CategorieNo = produit.CategorieNo,
                TypeCommande = produit.TypeCommande,
                TypeMesure = produit.TypeMesure,
                Prix = produit.Prix
            };
            return produitDeCatalogue;
        }

        public static ProduitDeCatalogue AvecEtat(Produit produit)
        {
            ProduitDeCatalogue produitDeCatalogue = ProduitDeCatalogue.SansEtatNiDate(produit);
            produitDeCatalogue.Etat = produit.Etat;
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
            ArchiveProduit produitInitial = archivesAvantDate[0];
            ProduitDeCatalogue produitDeCatalogue = new ProduitDeCatalogue(produitInitial.No)
            {
                Nom = produitInitial.Nom,
                CategorieNo = produitInitial.CategorieNo,
                TypeCommande = produitInitial.TypeCommande,
                TypeMesure = produitInitial.TypeMesure,
                Prix = produitInitial.Prix,
                Date = date
            };
            for (int i = 1; i < archivesAvantDate.Length; i++)
            {
                ArchiveProduit a = archivesAvantDate[i];
                if (a.Nom != null) { produitDeCatalogue.Nom = a.Nom; }
                if (a.CategorieNo != null) { produitDeCatalogue.CategorieNo = a.CategorieNo; }
                if (a.TypeCommande != null) { produitDeCatalogue.TypeCommande = a.TypeCommande; }
                if (a.TypeMesure != null) { produitDeCatalogue.TypeMesure = a.TypeMesure; }
                if (a.Prix != null) { produitDeCatalogue.Prix = a.Prix; }
            }
            return produitDeCatalogue;
        }
    }

    /// <summary>
    /// Contient tous les champs de données hors Etat et Date (rendus nullables) et le No d'un Produit.
    /// Objet envoyé en liste dans le catalogue des produits disponibles (client).
    /// </summary>
    public class ProduitDataSansEtatNiDate : IProduitDataSansEtatNiDate, IDataUidRnoNo
    {
        public long No { get; set; }

        // données
        public string Nom { get; set; }
        public long? CategorieNo { get; set; }

        public string TypeCommande { get; set; }
        public string TypeMesure { get; set; }
        public decimal? Prix { get; set; }

        protected ProduitDataSansEtatNiDate(long no)
        {
            No = no;
        }

        public ProduitDataSansEtatNiDate(Produit produit)
        {
            No = produit.No;
            ProduitFabrique.Copie(produit, this);
        }
        public virtual void Copie(Produit produit)
        {
            Nom = produit.Nom;
            CategorieNo = produit.CategorieNo;
            TypeCommande = produit.TypeCommande;
            TypeMesure = produit.TypeMesure;
            Prix = produit.Prix;
        }

    }

    /// <summary>
    /// Contient tous les champs de données hors Etat (rendus nullables) et le No d'un Produit.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class ProduitDataSansEtat : ProduitDataSansEtatNiDate, IProduitDataSansEtat, IDataUidRnoNo
    {

        public DateTime Date { get; set; }

        protected ProduitDataSansEtat(long no): base(no) { }

        public ProduitDataSansEtat(Produit produit, DateTime date): base(produit.No)
        {
            Date = date;
            var archives = produit.Archives
                .Where(a => a.Date <= date)
                .OrderBy(a => a.Date)
                .ToList();
            archives.ForEach(a => ProduitFabrique.CopieSiPasNullSansEtatNiDate(a, this));
        }
    }

    /// <summary>
    /// Contient tous les champs de données hors Date (rendus nullables) et le No d'un Produit.
    /// Objet envoyé en liste dans un catalogue de travail du fournisseur.
    /// </summary>
    public class ProduitDataSansDate : ProduitDataSansEtatNiDate, IProduitDataSansDate, IDataUidRnoNo
    {
        public string Etat { get; set; }

        protected ProduitDataSansDate(long no): base(no) { }

        public ProduitDataSansDate(Produit produit): base(produit.No)
        {
            Copie(produit);
        }
        public override void Copie(Produit produit)
        {
            base.Copie(produit);
            Etat = produit.Etat;
        }
    }

    /// <summary>
    /// Contient tous les champs de données (rendus nullables) et le No  d'un Produit.
    /// Objet envoyé en liste dans un catalogue à modifier.
    /// </summary>
    public class ProduitData : ProduitDataSansDate, IProduitDataSansDate, IDataUidRnoNo
    {
        public List<ProduitBilan> Bilans { get; set; }

        public ProduitData(long no): base(no) { }

        private ProduitBilan Bilan(Produit produit, string type)
        {
            var lignes = produit.Lignes.Where(l => l.Type == type);
            return new ProduitBilan
            {
                Type = type,
                Nb = lignes.Count(),
                Quantité = lignes.Where(l => l.Quantité.HasValue).Select(l => l.Quantité.Value).Sum()
            };
        }

        public ProduitData(Produit produit): base(produit)
        {
            Bilans = new List<ProduitBilan>
            {
                Bilan(produit, TypeClf.Commande),
                Bilan(produit, TypeClf.Livraison),
                Bilan(produit, TypeClf.Facture)
            };
        }
    }
}
