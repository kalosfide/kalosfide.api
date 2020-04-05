using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{
    public interface ICommandeService: IBaseService
    {
        #region Utiles

        /// <summary>
        /// retourne la commande définie par keyOuVueCommande avec les champs Livraison et Livraison.Facture inclus
        /// </summary>
        /// <param name="keyOuVueCommande"></param>
        /// <returns></returns>
        Task<Commande> CommandeDeKeyOuVue(AKeyUidRnoNo keyOuVueCommande);

        /// <summary>
        /// retourne la dernière commande du client défini par keyClient avec les champs Livraison et Livraison.Facture inclus
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        Task<Commande> DernièreCommande(AKeyUidRno keyClient);

        /// <summary>
        /// retourne la liste des commandes non vides sans numéro de livraison émises par les clients autorisés d'un site
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<List<Commande>> CommandesOuvertes(AKeyUidRno keySite);

        /// <summary>
        /// fixe le numéro de livraison des commandes sans numéro de livraison émises par !es clients autorisés d'un site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraisonNo"></param>
        /// <returns></returns>
        Task CommenceLivraison(Site site, long livraisonNo);

        /// <summary>
        /// supprime le numéro de livraison des commandes de la livraison
        /// </summary>
        /// <param name="site"></param>
        /// <param name="livraisonNo"></param>
        /// <returns></returns>
        Task AnnuleLivraison(Site site, long livraisonNo);

        /// <summary>
        /// supprime les commandes vides, fixe la date des autres à celle de leur dernier détail créé par le client s'il y en a, à celle de la livraison sinon.
        /// </summary>
        /// <param name="livraison"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        Task TermineLivraison(Site site, Livraison livraison );

        /// <summary>
        /// retourne les dernières CommandeVues des clients actifs ou nouveau du site défini par keySite
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<List<Commande>> DernièresCommandes(AKeyUidRno keySite);

        /// <summary>
        /// retourne les commandes reçues depuis la date
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<List<Commande>> CommandesOuvertesDesClientsAvecCompte(AKeyUidRno keySite);

        CommandeVue CréeCommandeVue(Commande commande);

        #endregion

        /// <summary>
        /// retourne un ContexteCommande contenant les données d'état définissant les droits
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <param name="site">Site du client</param>
        /// <returns>CommandesVue</returns>
        Task<ContexteCommande> Contexte(AKeyUidRno keyClient, Site site);

        /// <summary>
        /// retourne un CommandeVue contenant la dernière Commande d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns>CommandesVue</returns>
        Task<CommandeVue> EnCours(AKeyUidRno keyClient);

        Task<RetourDeService<Commande>> AjouteCommande(AKeyUidRno keyClient, long noCommande, Site site, bool estFournisseur);

        /// <summary>
        /// Supprime la commande et ses détails si la commande a été créée par l'utilisateur.
        /// Refuse la commande en fixant les ALivrer des détails à 0, si l'utilisateur est le fournisseur et la commande a été créée par le client.
        /// </summary>
        /// <param name="commande"></param>
        /// <param name="parLeClient">vrai si l'action est faite par le client</param>
        /// <returns></returns>
        Task<RetourDeService> SupprimeOuRefuse(Commande commande, bool parLeClient);

        /// <summary>
        /// retourne vrai si les TypeCommande du détail et du produit permettent la copie de Demande dans ALivrer
        /// </summary>
        /// <param name="détail"></param>
        /// <param name="produit"></param>
        /// <returns></returns>
        bool TypesPermettentCopieDemande(DétailCommande détail, Produit produit);

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande du site dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        Task<RetourDeService> CopieDemandes(Site site);

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande du client dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        Task<RetourDeService> CopieDemandes(Site site, KeyUidRno keyClient);

        /// <summary>
        /// Copie Demande dans AServir pour tous les DétailCommande demandant le produit dont la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyProduit"></param>
        /// <returns>null s'il n'y a pas de détails copiables</returns>
        Task<RetourDeService> CopieDemandes(Site site, KeyUidRnoNo keyProduit);

        /// <summary>
        /// Copie Demande dans AServir pour le DétailCommande si la demande est copiable
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyDétail"></param>
        /// <returns>null si le détail n'est pas copiable</returns>
        Task<RetourDeService> CopieDemandes(Site site, KeyUidRnoNo2 keyDétail);

    }
}
