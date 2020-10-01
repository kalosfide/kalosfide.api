using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public interface IUtilisateurService : IBaseService<Utilisateur>
    {
        Task<ApplicationUser> TrouveParId(string userId);
        Task<ApplicationUser> TrouveParNom(string userName);
        Task<ApplicationUser> TrouveParEmail(string eMail);
        Task<ApplicationUser> ApplicationUserVérifié(string userName, string password);

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

        Task Connecte(ApplicationUser user, bool persistant);
        Task Déconnecte();

        Task<CarteUtilisateur> CréeCarteUtilisateur(ApplicationUser applicationUser);
        Task<CarteUtilisateur> CréeCarteUtilisateur(ClaimsPrincipal user);

        bool VérifieTrimCréeSiteVue(ICréeSiteVue vue);
        Task<RetourDeService<Role>> CréeRoleSite(string uid, ICréeSiteVue vue);


        /// <summary>
        /// Trouve le client défini par la clé. Retourne null si le client n'existe pas où n'appartient pas au site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="uidClient"></param>
        /// <param name="rno"></param>
        /// <returns></returns>
        Task<Client> TrouveClientDansSite(Site site, string uidClient, int rnoClient);
        
        Task<Invitation> TrouveInvitation(IInvitationKey key);
        Task<List<InvitationVue>> Invitations(Site site);
        Task<RetourDeService<Invitation>> SupprimeInvitation(Invitation invitation);
        Task<RetourDeService<Invitation>> RemplaceInvitation(Invitation enregistrée, Invitation invitation);
        Task EnvoieEmailDevenirClient(Invitation invitation, InvitationVérifiée vérifiée);
        Invitation DécodeInvitation(string code);
        Task<Client> ClientInvité(string uid, int rno);

        bool VérifieTrimDevenirClientVue(DevenirClientVue vue);
        Task<bool> VérifieNomPris(Site site, string Nom, Client clientInvité);
        Task<RetourDeService> CréeRoleClient(Site site, Utilisateur utilisateur, Client clientInvité, DevenirClientVue vue);

        Task<RetourDeService<ApplicationUser>> CréeUtilisateur(ICréeCompteVue vue);
        Task<RetourDeService<Utilisateur>> CréeUtilisateur();
        Task<Utilisateur> CréeUtilisateurSansSauver();

        Task<Utilisateur> Lit(string id);

        Task<List<Utilisateur>> Lit();

        Task<Utilisateur> UtilisateurDeUser(string userId);

        Task<RetourDeService<Utilisateur>> Supprime(Utilisateur utilisateur);

        Task<bool> UserNamePris(string userName);
        Task<bool> EmailPris(string eMail);
    }
}