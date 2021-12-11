using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public interface IUtilisateurService : IBaseService<Utilisateur>
    {
        /// <summary>
        /// Cherche un ApplicationUser à partir de son Id.
        /// </summary>
        /// <param name="userId">Id de l'ApplicationUser recherché</param>
        /// <returns>l'ApplicationUser trouvé, ou null.</returns>
        Task<ApplicationUser> ApplicationUserDeUserId(string userId);

        /// <summary>
        /// Cherche un ApplicationUser à partir de son Email.
        /// </summary>
        /// <param name="eMail">Email de l'ApplicationUser recherché</param>
        /// <returns>l'ApplicationUser trouvé, ou null.</returns>
        Task<ApplicationUser> ApplicationUserDeEmail(string eMail);

        /// <summary>
        /// Cherche un ApplicationUser à partir de son Email et vérifie son mot de passe.
        /// </summary>
        /// <param name="eMail">Email de l'ApplicationUser recherché</param>
        /// <param name="password">mot de passe à vérifier</param>
        /// <returns>l'ApplicationUser trouvé si le mot de passe correspond à l'email, ou null.</returns>
        Task<ApplicationUser> ApplicationUserVérifié(string eMail, string password);

        /// <summary>
        /// Cherche un Utilisateur à partir de son ApplicationUser.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>l'Utilisateur trouvé qui inclut ses Roles qui incluent leurs Site, ou null.</returns>
        Task<Utilisateur> UtilisateurDeApplicationUser(ApplicationUser user);

        /// <summary>
        /// Cherche un Utilisateur à partir de son Uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>l'Utilisateur trouvé qui inclut son ApplicationUser, ou null.</returns>
        Task<Utilisateur> UtilisateurDeUid(string id);

        /// <summary>
        /// Cherche un Role à partir de sa KeyUidRno.
        /// </summary>
        /// <param name="iKeyRole">objet ayant l'Uid et le Rno du Role recherché</param>
        /// <returns>le Role trouvé qui inclut son Site, ou null.</returns>
        Task<Role> RoleDeKey(IKeyUidRno iKeyRole);

        /// <summary>
        /// Crée une CarteUtilisateur à partir d'un ApplicationUser.
        /// Fixe l'Utilisateur de la carte avec son ApplicationUser et ses Roles incluant leurs Sites et leurs Archives.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>la CarteUtilisateur créée</returns>
        Task<CarteUtilisateur> CréeCarteUtilisateur(ApplicationUser user);

        /// <summary>
        /// Crée une CarteUtilisateur à partir des Claims envoyées avec une requête Http.
        /// Fixe l'Utilisateur de la carte avec son ApplicationUser et ses Roles incluant leurs Sites.
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
        /// Ajoute à la bdd un nouvel Utilisateur et son ApplicationUser à partir de son Email et de son mot de passe.
        /// </summary>
        /// <param name="vue">objet ayant l'Email et le Password de l'utilisateur à créer</param>
        /// <returns></returns>
        Task<RetourDeService<ApplicationUser>> CréeUtilisateur(ICréeCompteVue vue);

        /// <summary>
        /// Ajoute à la bdd un nouvel Utilisateur sans ApplicationUser.
        /// </summary>
        /// <returns></returns>
        Task<RetourDeService<Utilisateur>> CréeUtilisateur();

        /// <summary>
        /// Supprime dans la bdd un Utilisateur et son ApplicationUser s'il en a un.
        /// </summary>
        /// <param name="utilisateur">Utilisateur à supprimer</param>
        /// <returns></returns>
        Task<RetourDeService> Supprime(Utilisateur utilisateur);

        Task EnvoieEmailConfirmeCompte(ApplicationUser user);

        Task EnvoieEmailRéinitialiseMotDePasse(ApplicationUser user);
        Task<bool> RéinitialiseMotDePasse(ApplicationUser user, string code, string motDePasse);

        Task<bool> ChangeMotDePasse(ApplicationUser user, string motDePasse, string nouveauMotDePasse);

        Task EnvoieEmailChangeEmail(ApplicationUser user, string nouvelEmail);
        Task<bool> ChangeEmail(ApplicationUser user, string nouvelEmail, string code);

        /// <summary>
        /// Retourne vrai si la confimation de l'email a eu lieu
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<bool> EmailConfirmé(ApplicationUser user, string code);

        Task ConfirmeEmailDirect(ApplicationUser user);

        Task Connecte(CarteUtilisateur carteUtilisateur);
        Task Déconnecte(CarteUtilisateur carteUtilisateur);
        
        Task<Invitation> TrouveInvitation(IInvitationKey key);
        Task<List<InvitationVue>> Invitations(AKeyUidRno keySite);
        Task<RetourDeService<Invitation>> SupprimeInvitation(Invitation invitation);
        Task<RetourDeService<Invitation>> EnvoieEmailDevenirClient(Invitation invitation, InvitationVérifiée vérifiée);
        Invitation DécodeInvitation(string code);

        Task<List<Utilisateur>> Lit();

    }
}