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
        /// Retourne une fonction qui filtre les roles qui appartiennent à un site
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Func<Role, bool> FiltreSite(AKeyUidRno keySite);

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
        /// retourne la date du catalogue du site
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<DateTime> DateCatalogue(AKeyUidRno keySite);

        /// <summary>
        /// retourne un filtre que ne passent que les roles d'Etat Actif ou Nouveau
        /// </summary>
        /// <returns></returns>
        Func<Role, bool> FiltreRoleActif();

        /// <summary>
        /// Retourne un IQueryable qui renvoie les Clients passant les filtres présents et qui inclut les champs Role et Role.Site
        /// </summary>
        /// <param name="filtreClient">si présent, le Client doit passer le filtre</param>
        /// <param name="filtreRole">si présent, le role doit passer le filtre</param>
        /// <param name="keySite">si présent, le site du role doit être celui de keySite</param>
        /// <returns></returns>
        IQueryable<Client> ClientsAvecRoleEtSite(Func<Client, bool> filtreClient, Func<Role, bool> filtreRole, AKeyUidRno keySite);

        Task<Produit> Produit(Site site, long No);
    }
}
