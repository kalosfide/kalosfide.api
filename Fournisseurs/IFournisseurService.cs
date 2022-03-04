using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    public interface IFournisseurService: IAvecIdUintService<Fournisseur, FournisseurAAjouter, Fournisseur, FournisseurAEditer>
    {
        /// <summary>
        /// Cherche une demande de création de site à partir de l'Email.
        /// </summary>
        /// <param name="email">Email de la DemandeSite cherchée</param>
        /// <returns>si elle existe, la DemandeSite retournée n'inclut pas son Fournisseur</returns>
        Task<DemandeSite> DemandeSite(string email);

        /// <summary>
        /// Cherche une demande de création de site identique à une demande.
        /// </summary>
        /// <param name="demande">DemandeSite cherchée</param>
        /// <returns>une DemandeSite qui inclut son Fournisseur avec son Site, si trouvée; null, sinon.</returns>
        Task<DemandeSite> DemandeSiteIdentique(DemandeSite demande);
        new

        /// <summary>
        /// Ajoute une DemandeSite avec son Fournisseur et son Site. 
        /// </summary>
        /// <param name="ajout"></param>
        /// <param name="modelState">ModelStateDictionary où inscrire les erreurs de validation</param>
        /// <returns>un Objet contenant l'Id commun aux objets ajoutés et la Date de l'ajout.</returns>
        Task<RetourDeService<DemandeSiteDate>> Ajoute(FournisseurAAjouter ajout, ModelStateDictionary modelState);

        /// <summary>
        /// Retourne la liste des DemandeSiteVue des DemandeSite enregistrées avec leurs Fournissuers.
        /// </summary>
        /// <returns></returns>
        Task<List<DemandeSiteVue>> Demandes();

        /// <summary>
        /// Supprime une DemandeSite et son Fournisseur de la bdd.
        /// </summary>
        /// <param name="demande">DemandeSite à supprimer</param>
        /// <returns></returns>
        Task<RetourDeService> Annule(DemandeSite demande);

        /// <summary>
        /// Supprime une DemandeSite mais pas son Fournisseur de la bdd.
        /// </summary>
        /// <param name="demande">DemandeSite à supprimer</param>
        /// <returns></returns>
        Task<RetourDeService> Supprime(DemandeSite demande);

        /// <summary>
        /// Fixe la date d'envoi de la demande et envoie un message à l'Email de la demande avec un lien contenant la DemandeSite encodée.
        /// </summary>
        /// <param name="demande">DemandeSite à envoyer</param>
        /// <returns></returns>
        Task<RetourDeService<DemandeSiteEnvoi>> EnvoieEmailDemandeSite(DemandeSite demande);

        /// <summary>
        /// DemandeSite contenue dans le code du lien envoyé dans le message email d'invitation.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>la DemandeSite contenue dans le code, si le code est valide; null, sinon</returns>
        DemandeSite DécodeDemandeSite(string code);

        /// <summary>
        /// Fixe l'UtilisateurId du Fournisseur et enregistre dans la bdd.
        /// </summary>
        /// <param name="fournisseur">Fournisseur d'une DemandeSite à activer</param>
        /// <param name="utilisateur">Utilisateur affecté à ce Fournisseur</param>
        /// <returns></returns>
        Task<RetourDeService> FixeUtilisateur(Fournisseur fournisseur, Utilisateur utilisateur);

        /// <summary>
        /// Retourne la liste des FournisseurVue des Fournissuers qui ont un Utilisateur.
        /// </summary>
        /// <returns></returns>
        Task<List<FournisseurVue>> Fournisseurs();

        Task<FournisseurVue> LitFournisseur(uint idFournisseur);

        Task<RetourDeService<RoleEtat>> ChangeEtat(Fournisseur fournisseur, EtatRole etat);
    }
}
