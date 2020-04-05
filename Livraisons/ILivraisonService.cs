using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Livraisons
{
    public interface ILivraisonService : IBaseService
    {

        Task<LivraisonVue> LivraisonVueEnCours(Site site);

        /// <summary>
        /// retourne un objet LivraisonVue contenant les commandes reçues depuis la date
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<LivraisonVue> VueDesCommandesOuvertesDesClientsAvecCompte(Site site);

        /// <summary>
        /// retourne vrai si tous les ALivrer des détails des commandes de la livraison sont fixés
        /// </summary>
        /// <param name="livraison"></param>
        /// <returns></returns>
        Task<bool> EstPrête(Livraison livraison);

        /// <summary>
        /// retourne la dernière livraison du site si elle n'est pas terminée
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Livraison> LivraisonPasTerminée(Site site);

        /// <summary>
        /// retourne la dernière livraison du site si elle n'est pas terminée
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Livraison> LivraisonATerminer(Site site);

        /// <summary>
        /// crée une livraison et fixe le numéro de livraison des commandes sans numéro de livraison
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task CommenceLivraison(Site site);

        /// <summary>
        /// supprime une livraison et supprime le numéro de livraison des commandes qui y étaient affectées
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraison"></param>
        /// <returns></returns>
        Task AnnuleLivraison(Site site, Livraison livraison);

        /// <summary>
        /// fixe la date de la livraison, supprime les commandes vides de la livraison et fixe la date des autres
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraison"></param>
        /// <returns></returns>
        Task TermineLivraison(Site site, Livraison livraison);
    }
}
