using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public interface IUtilisateurService : IBaseService
    {
        /// <summary>
        /// Cherche un Utilisateur à partir de son Id.
        /// </summary>
        /// <param name="id">Id de l'Utilisateur recherché</param>
        /// <returns>l'Utilisateur trouvé, ou null.</returns>
        Task<Utilisateur> UtilisateurDeId(string id);

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email.
        /// </summary>
        /// <param name="eMail">Email de l'Utilisateur recherché</param>
        /// <returns>l'Utilisateur trouvé, ou null.</returns>
        Task<Utilisateur> UtilisateurDeEmail(string eMail);

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email et vérifie son mot de passe.
        /// </summary>
        /// <param name="eMail">Email de l'Utilisateur recherché</param>
        /// <param name="password">mot de passe à vérifier</param>
        /// <returns>l'Utilisateur trouvé si le mot de passe correspond à l'email, ou null.</returns>
        Task<Utilisateur> UtilisateurVérifié(string eMail, string password);

        /// <summary>
        /// Complète un Utilisateur à ses roles.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>l'Utilisateur trouvé qui inclut ses roles de Fournisseur et de Client qui incluent leurs Site, ou null.</returns>
        Task<Utilisateur> UtilisateurAvecRoles(Utilisateur user);

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email et, s'il existe, vérifie s'il est usager d'un Site.
        /// </summary>
        /// <param name="email">Email de l'Utilisateur à chercher</param>
        /// <param name="idSite">Id d'un Site</param>
        /// <returns>true, si l'Utilisateur existe et est usager du Site; false, sinon.</returns>
        Task<bool> UtilisateurDeEmailEstUsagerDeSite(string email, uint idSite);

        /// <summary>
        /// Crée une CarteUtilisateur à partir d'un Utilisateur.
        /// Fixe l'Utilisateur de la carte avec son Utilisateur et ses Roles incluant leurs Sites et leurs Archives.
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns>la CarteUtilisateur créée</returns>
        Task<CarteUtilisateur> CréeCarteUtilisateur(Utilisateur utilisateur);

        /// <summary>
        /// Crée une CarteUtilisateur à partir des Claims envoyées avec une requête Http.
        /// Fixe l'Utilisateur de la carte et ses Roles incluant leurs Sites.
        /// </summary>
        /// <param name="httpContext">le HttpContext de la requête</param>
        /// <returns>null si les Claims ne sont pas valide</returns>
        Task<CarteUtilisateur> CréeCarteUtilisateur(HttpContext httpContext);

        /// <summary>
        /// Ajoute un header contenant le jeton identifiant de la carte à la response d'une requête de connection
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="carte"></param>
        /// <returns></returns>
        Task AjouteCarteAResponse(HttpResponse httpResponse, CarteUtilisateur carte);

        /// <summary>
        /// Ajoute à la bdd un nouvel Utilisateur à partir de son Email et de son mot de passe.
        /// </summary>
        /// <param name="vue">objet ayant l'Email et le Password de l'utilisateur à créer</param>
        /// <returns></returns>
        Task<RetourDeService<Utilisateur>> CréeUtilisateur(ICréeCompteVue vue);

        /// <summary>
        /// Supprime dans la bdd un Utilisateur s'il en a un.
        /// </summary>
        /// <param name="utilisateur">Utilisateur à supprimer</param>
        /// <returns></returns>
        Task<RetourDeService> Supprime(Utilisateur utilisateur);

        Task EnvoieEmailConfirmeCompte(Utilisateur user);

        TokenDaté DécodeTokenDaté(string code);

        /// <summary>
        /// Durée de validité d'un PasswordResetToken généré par l'UserManager.
        /// </summary>
        TimeSpan DuréeValiditéTokenRéinitialiseMotDePasse { get; }

        /// <summary>
        /// Envoie un email avec un lien ayant en QueryParams l'Id de l'utilisateur et un code représentant le cryptage
        /// d'un TokenDaté contenant un PasswordResetToken généré par l'UserManager et la date d'envoi.
        /// </summary>
        /// <param name="utilisateur">Utilisateur dont on veut réinitialiser le mot de passe</param>
        /// <returns></returns>
        Task EnvoieEmailRéinitialiseMotDePasse(Utilisateur user);

        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur.
        /// </summary>
        /// <param name="id">Id de Utilisateur dont on veut réinitialiser le mot de passe</param>
        /// <param name="token">PasswordResetToken généré par l'UserManager</param>
        /// <param name="motDePasse"></param>
        /// <returns>true, si le mot de passe a été changé; false, sinon.</returns>
        Task<bool> RéinitialiseMotDePasse(string id, string token, string motDePasse);

        Task<bool> ChangeMotDePasse(Utilisateur user, string motDePasse, string nouveauMotDePasse);

        /// <summary>
        /// Durée de validité d'un EmailToken généré par l'UserManager.
        /// </summary>
        TimeSpan DuréeValiditéTokenChangeEmail { get; }
        Task EnvoieEmailChangeEmail(Utilisateur user, string nouvelEmail);

        /// <summary>
        /// Change l'Email d'un Utilisateur.
        /// </summary>
        /// <param name="id">Id de Utilisateur qui veut changer d'Email</param>
        /// <param name="nouvelEmail"></param>
        /// <param name="token">EmailToken généré par l'UserManager</param>
        /// <returns></returns>
        Task<bool> ChangeEmail(string id, string nouvelEmail, string token);

        /// <summary>
        /// Retourne vrai si la confimation de l'email a eu lieu
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<bool> EmailConfirmé(Utilisateur user, string code);

        Task ConfirmeEmailDirect(Utilisateur user);

        Task<RetourDeService> Connecte(Utilisateur utilisateur);
        Task Connecte(CarteUtilisateur carteUtilisateur);
        Task Déconnecte(CarteUtilisateur carteUtilisateur);
        
        Task<List<Utilisateur>> Lit();

        Task<RetourDeService> FixeIdDernierSite(Utilisateur utilisateur, uint id);

        /// <summary>
        /// Fixe l'UtilisateurId du Fournisseur et enregistre dans la bdd.
        /// </summary>
        /// <param name="fournisseur">Fournisseur d'une DemandeSite à activer</param>
        /// <param name="utilisateur">Utilisateur affecté à ce Fournisseur</param>
        /// <returns></returns>
        Task<RetourDeService> FixeUtilisateur(Fournisseur fournisseur, Utilisateur utilisateur);

    }
}