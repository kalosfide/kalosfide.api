using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Catégories
{

    /// <summary>
    /// Contient tous les champs de données hors Date d'une Catégorie.
    /// </summary>
    public interface ICatégorieDataSansDate : IDataUidRnoNo
    {
        string Nom { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données d'une Catégorie.
    /// </summary>
    public interface ICatégorieData : ICatégorieDataSansDate
    {
        DateTime Date { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données hors Date et la KeyUidRnoNo d'une Catégorie.
    /// Objet reçu pour ajouter ou éditer une Catégorie.
    /// </summary>
    public class CatégorieVue: AKeyUidRnoNo, ICatégorieDataSansDate
    {
        // identité
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public override long No { get; set; }

        // données
        public string Nom { get; set; }

        public CatégorieVue()
        { }

        public CatégorieVue(Catégorie catégorie)
        {
            CopieKey(catégorie);
            Nom = catégorie.Nom;
        }

    }

    /// <summary>
    /// Contient tous les champs de données avec Date et le No d'une Catégorie.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieDeCatalogue
    {
        [JsonProperty]
        public long No { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        private CatégorieDeCatalogue(long no)
        {
            No = no;
        }

        public static CatégorieDeCatalogue SansDate(Catégorie catégorie)
        {
            CatégorieDeCatalogue catégorieDeCatalogue = new CatégorieDeCatalogue(catégorie.No)
            {
                Nom = catégorie.Nom
            };
            return catégorieDeCatalogue;
        }

        /// <summary>
        /// Retrouve l'état d'une catégorie à une date passée.
        /// </summary>
        /// <param name="archives">archives d'une catégorie</param>
        /// <param name="date">date d'une fin de modification de catalogue passée</param>
        /// <returns></returns>
        public static CatégorieDeCatalogue ALaDate(IEnumerable<ArchiveCatégorie> archives, DateTime date)
        {
            ArchiveCatégorie[] archivesAvantDate = archives.Where(a => a.Date < date).OrderBy(a => a.Date).ToArray();
            ArchiveCatégorie catégorieInitiale = archivesAvantDate[0];
            CatégorieDeCatalogue catégorieDeCatalogue = new CatégorieDeCatalogue(catégorieInitiale.No)
            {
                Nom = catégorieInitiale.Nom,
                Date = date
            };
            for (int i = 1; i < archivesAvantDate.Length; i++)
            {
                ArchiveCatégorie a = archivesAvantDate[i];
                if (a.Nom != null) { catégorieDeCatalogue.Nom = a.Nom; }
            }
            return catégorieDeCatalogue;
        }
    }

    /// <summary>
    /// Contient tous les champs de données hors Date et le No d'une Catégorie.
    /// Objet envoyé en liste dans un catalogue.
    /// </summary>
    public class CatégorieDataSansDate: ICatégorieDataSansDate
    {
        public long No { get; set; }

        public string Nom { get; set; }

        protected CatégorieDataSansDate(long no)
        {
            No = no;
        }

        public CatégorieDataSansDate(Catégorie catégorie)
        {
            No = catégorie.No;
            Nom = catégorie.Nom;
        }
    }

    /// <summary>
    /// Contient tous les champs de données avec Date et le No d'une Catégorie.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieData: CatégorieDataSansDate, ICatégorieData
    {

        public DateTime Date { get; set; }

        protected CatégorieData(long no): base(no)
        {

        }

        public CatégorieData(Catégorie catégorie): base(catégorie)
        {
            Date = catégorie.Date;
        }

        public CatégorieData(Catégorie catégorie, DateTime date): base(catégorie.No)
        {
            Date = date;
            catégorie.Archives
                .Where(a => a.Date <= date)
                .OrderBy(a => a.Date)
                .ToList()
                .ForEach(a => CopieSiPasNull(a));
        }

        public void CopieSiPasNull(ICatégorieDataSansDate data)
        {
            Nom = data.Nom;
        }
    }
}
