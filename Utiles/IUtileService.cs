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

        Task<Site> SiteDeKey(AKeyUidRno akeySite);

        /// <summary>
        /// retourne le Site du Role défini par keyClient si le Role n'est pas celui du Fournisseur du Site
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        Task<Site> SiteDeClient(AKeyUidRno keyClient);

        /// <summary>
        /// retourne le site du produit ou de la livraison
        /// </summary>
        /// <param name="keyProduitOuLivraison"></param>
        /// <returns></returns>
        Task<Site> SiteDeKeyProduitOuLivraison(KeyUidRnoNo keyProduitOuLivraison);

        /// <summary>
        /// retourne le nombre de produits disponibles du site
        /// </summary>
        /// <param name="keySite">Site ou SiteVue ou keyUidRno</param>
        /// <returns></returns>
        Task<int> NbDisponibles(AKeyUidRno keySite);

        /// <summary>
        /// Retourne le Role (qui inclut le champ Site) correspondant à la key s'il sagit de celui d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns></returns>
        Task<Role> ClientRoleAvecSite(AKeyUidRno keyClient);

        Task<Produit> Produit(Site site, long No);
    }
}
