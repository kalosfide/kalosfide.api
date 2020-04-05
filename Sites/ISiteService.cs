using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{
    public interface ISiteService : IKeyUidRnoService<Site, SiteVue>
    {
        Site CréeSite(Role role, EnregistrementFournisseurVue fournisseurVue);
        /// <summary>
        /// Retourne une vue ne contenant que l'état
        /// </summary>
        /// <param name="akeySite"></param>
        /// <returns></returns>
        Task<SiteVue> Etat(AKeyUidRno akeySite);

        /// <summary>
        /// vérifie que le changement de l'état du site pour celui de la vue est possible
        /// inscrit les erreurs dans le ModelSate du Controller
        /// </summary>
        /// <param name="site"></param>
        /// <param name="vue"></param>
        /// <param name="modelState"></param>
        /// <param name="estAdministrateur"></param>
        /// <returns></returns>
        Task ValideChangeEtat(Site site, SiteVue vue, ModelStateDictionary modelState, bool estAdministrateur);

        /// <summary>
        /// change l'état du site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="état"></param>
        /// <returns></returns>
        Task<RetourDeService> ChangeEtat(Site site, string état);

        Task<SiteVue> TrouveParNom(string nomSite);

        Task FixeNbs(List<SiteVue> siteVues);
        Task<SiteVue> LitNbs(Site site);

        Task<bool> NomPris(string nomSite);
        Task<bool> NomPrisParAutre(AKeyUidRno key, string nomSite);
        Task<bool> TitrePris(string titre);
        Task<bool> TitrePrisParAutre(AKeyUidRno key, string titre);
    }
}
