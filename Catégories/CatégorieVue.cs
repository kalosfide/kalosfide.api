using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Catégories
{
    /// <summary>
    /// Contient tous les champs d'une Catégorie sans Date sans Id avec SiteId.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieAAjouter: ICatégorieData
    {
        public uint SiteId { get; set; }
        public string Nom { get; set; }

    }
    /// <summary>
    /// Contient tous les champs d'une Catégorie sans Date avec Id sans SiteId.
    /// Contient tous les champs de données avec Date et Id d'une Catégorie sans SiteId.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieAEditer: AvecIdUint, ICatégorieDataAnnulable
    {
        public string Nom { get; set; }


    }

    /// <summary>
    /// Contient tous les champs d'une Catégorie avec Date et Id sans SiteId.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieDeCatalogue: AvecIdUint, ICatégorieData
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        private CatégorieDeCatalogue(uint id)
        {
            Id = id;
        }

        public static CatégorieDeCatalogue SansDate(Catégorie catégorie)
        {
            CatégorieDeCatalogue catégorieDeCatalogue = new CatégorieDeCatalogue(catégorie.Id);
            Catégorie.CopieData(catégorie, catégorieDeCatalogue);
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
            CatégorieDeCatalogue catégorieDeCatalogue = new CatégorieDeCatalogue(archivesAvantDate.First().Id)
            {
                Date = date
            };
            foreach (ArchiveCatégorie archive in archivesAvantDate)
            {
                Catégorie.CopieDataSiPasNull(archive, catégorieDeCatalogue);
            }
            return catégorieDeCatalogue;
        }
    }

}
