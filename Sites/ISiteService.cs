using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{
    public interface ISiteService : IAvecIdUintService<Site, ISiteData, SiteAEditer>
    {

        GèreArchiveSite GèreArchiveSite { get; }

        /// <summary>
        /// Enregistre le commencement de la modification du catalogue du site.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<RetourDeService<ArchiveSite>> CommenceEtatCatalogue(Site site);

        /// <summary>
        /// Enregistre la fin de la modification du catalogue du site.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="dateCatalogue">présent si le catalogue a été modifié</param>
        /// <returns></returns>
        Task<RetourDeService<ArchiveSite>> TermineEtatCatalogue(Site site, DateTime? dateCatalogue);

        Task<Site> TrouveParUrl(string url);
        Task<Site> TrouveParId(uint id);

        /// <summary>
        /// Vérifie s'il existe un Site ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        Task<bool> UrlPrise(string url);

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par l'Id ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <param name="id">Id d'un Site</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        Task<bool> UrlPriseParAutre(uint id, string url);

        /// <summary>
        /// Vérifie s'il existe un Site ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        Task<bool> TitrePris(string titre);

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par l'Id ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <param name="id">Id d'un Site</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        Task<bool> TitrePrisParAutre(uint id, string titre);
    }
}
