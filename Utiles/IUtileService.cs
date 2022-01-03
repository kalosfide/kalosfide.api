using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public interface IUtileService
    {

        /// <summary>
        /// retourne le nombre de produits disponibles du site
        /// </summary>
        /// <param name="idSite">Id du Site</param>
        /// <returns></returns>
        Task<int> NbDisponibles(uint idSite);

        /// <summary>
        /// Cherche un Client à partir de son Id.
        /// </summary>
        /// <param name="idClient">Id du client</param>
        /// <returns>le Client qui inclut son Site, si trouvé; null, sinon</returns>
        Task<Client> ClientAvecSite(uint idClient);

        /// <summary>
        /// Cherche un Produit à partir de son Id.
        /// </summary>
        /// <param name="idProduit">Id du Produit</param>
        /// <returns>le Produit qui inclut ses Archives, si trouvé; null, sinon</returns>
        Task<Produit> Produit(uint idProduit);
    }
}
