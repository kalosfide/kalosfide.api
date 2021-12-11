using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.NouveauxSites
{
    public interface INouveauSiteService
    {
        /// <summary>
        /// Cherche un NouveauSite à partir de son Email
        /// </summary>
        /// <param name="email">Email du NouveauSite recherché</param>
        /// <returns>le NouveauSite trouvé, ou null.</returns>
        Task<NouveauSite> NouveauSite(string email);

        /// <summary>
        /// Vérifie s'il existe un NouveauSite ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du NouveauSite recherché</param>
        /// <returns>true s'il existe un NouveauSite ayant cette Url</returns>
        Task<bool> UrlPrise(string url);

        /// <summary>
        /// Vérifie s'il existe un NouveauSite ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du NouveauSite recherché</param>
        /// <returns>true s'il existe un NouveauSite ayant ce Titre</returns>
        Task<bool> TitrePris(string titre);

        /// <summary>
        /// Enregistre dans la bdd un NouveauSite créé à partir de la vue.
        /// </summary>
        /// <param name="vue"></param>
        /// <returns></returns>
        Task<RetourDeService> EnregistreDemande(NouveauSiteDemande vue);

        /// <summary>
        /// Ajoute à la bdd un Role et un Site créés à partir d'un NouveauSite.
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="nouveauSite"></param>
        /// <returns>un RetourDeService contenant le Role créé incluant son Site ou une erreur</returns>
        Task<RetourDeService<Role>> CréeRoleEtSite(Utilisateur utilisateur, NouveauSite nouveauSite);
    }
}
