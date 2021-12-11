using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{
    public interface ISiteService : IKeyUidRnoService<Site, SiteVue>
    {
        Task<List<SiteVue>> ListeVues();

        Task<RetourDeService<Role>> CréeRoleSite(Utilisateur utilisateur, ICréeSiteVue vue);

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
        Task<Site> TrouveParKey(string Uid, int Rno);

        /// <summary>
        /// Vérifie s'il existe un Site ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        Task<bool> UrlPrise(string url);

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par la KeyUidRno ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <param name="key">Objet ayant la KeyUidRno d'un Site</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        Task<bool> UrlPriseParAutre(AKeyUidRno key, string url);

        /// <summary>
        /// Vérifie s'il existe un Site ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        Task<bool> TitrePris(string titre);

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par la KeyUidRno ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <param name="key">Objet ayant la KeyUidRno d'un Site</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        Task<bool> TitrePrisParAutre(AKeyUidRno key, string titre);
    }
}
