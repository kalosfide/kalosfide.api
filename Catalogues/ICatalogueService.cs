using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public interface ICatalogueService
    {


        /// <summary>
        /// Archive les changements du catalogue depuis le début de la modification.
        /// </summary>
        /// <param name="site"></param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        Task<bool> ArchiveModifications(Site site, DateTime maintenant);

        /// <summary>
        /// retourne le catalogue complet du site actuellement en vigueur
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Catalogue> Complet(Site site);

        /// <summary>
        /// retourne le catalogue des disponibilités du site actuellement en vigueur
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Catalogue> Disponibles(Site site);

    }
}
