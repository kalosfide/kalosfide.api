using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Produits
{
    public class ProduitVue: AKeyUidRnoNo, IProduitData
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

        // calculés
        public string NomCategorie { get; set; }

        public DateTime? Date { get; set; }
    }
    public interface IProduitData : IDataUidRnoNo
    {
        // données
        string Nom { get; set; }
        long? CategorieNo { get; set; }

        string TypeCommande { get; set; }
        string TypeMesure { get; set; }
        decimal? Prix { get; set; }

        string Etat { get; set; }

        // calculés
        string NomCategorie { get; set; }

        DateTime? Date { get; set; }
    }
    public class ProduitData : IProduitData, IDataUidRnoNo
    {
        public long No { get; set; }

        // données
        public string Nom { get; set; }
        public long? CategorieNo { get; set; }

        public string TypeCommande { get; set; }
        public string TypeMesure { get; set; }
        public decimal? Prix { get; set; }

        public string Etat { get; set; }

        public List<ProduitBilan> Bilans { get; set; }

        // calculés
        public string NomCategorie { get; set; }

        public DateTime? Date { get; set; }
    }
}
