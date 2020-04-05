using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using System;

namespace KalosfideAPI.Catégories
{
    public class CatégorieVue: AKeyUidRnoNo, ICatégorieData
    {
        // identité
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public override long No { get; set; }

        // données
        public string Nom { get; set; }

        public string Etat { get; set; }

        // calculés
        public int NbProduits { get; set; }

        public DateTime? Date { get; set; }
    }

    public interface ICatégorieData : IDataUidRnoNo
    {
        string Nom { get; set; }

        DateTime? Date { get; set; }
    }
    public class CatégorieData: ICatégorieData
    {
        public long No { get; set; }

        public string Nom { get; set; }

        public DateTime? Date { get; set; }
    }
}
