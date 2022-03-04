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
    /// </summary>
    public class CatégorieAAjouter: ICatégorieData
    {
        public uint SiteId { get; set; }
        public string Nom { get; set; }

    }
    /// <summary>
    /// Contient tous les champs d'une Catégorie sans Date avec Id sans SiteId.
    /// </summary>
    public class CatégorieAEditer: AvecIdUint, ICatégorieDataAnnulable
    {
        public string Nom { get; set; }


    }

    /// <summary>
    /// Contient tous les champs d'une Catégorie avec Date et Id sans SiteId.
    /// Objet envoyé en liste dans un tarif.
    /// </summary>
    public class CatégorieAEnvoyer: AvecIdUint, ICatégorieData
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Nom { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        private CatégorieAEnvoyer(uint id)
        {
            Id = id;
        }

        public static CatégorieAEnvoyer SansDate(Catégorie catégorie)
        {
            CatégorieAEnvoyer catégorieDeCatalogue = new CatégorieAEnvoyer(catégorie.Id);
            Catégorie.CopieData(catégorie, catégorieDeCatalogue);
            return catégorieDeCatalogue;
        }

        /// <summary>
        /// Retrouve l'état d'une catégorie à une date passée.
        /// </summary>
        /// <param name="archives">archives d'une catégorie</param>
        /// <param name="date">date d'une fin de modification de catalogue passée</param>
        /// <returns></returns>
        public static CatégorieAEnvoyer ALaDate(IEnumerable<ArchiveCatégorie> archives, DateTime date)
        {
            ArchiveCatégorie[] archivesAvantDate = archives.Where(a => a.Date < date).OrderBy(a => a.Date).ToArray();
            CatégorieAEnvoyer catégorieDeCatalogue = new CatégorieAEnvoyer(archivesAvantDate.First().Id)
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
