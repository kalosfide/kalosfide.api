using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Data.Constantes;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Utiles;

namespace KalosfideAPI.Utilisateurs
{

    public class UtilisateurService : BaseService<Utilisateur>, IUtilisateurService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtFabrique _jwtFabrique;
        private readonly IEnvoieEmailService _emailService;

        public UtilisateurService(
            ApplicationContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtFabrique jwtFabrique,
            IEnvoieEmailService emailService
            ) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtFabrique = jwtFabrique;
            _emailService = emailService;
        }

        #region Recherche

        /// <summary>
        /// Cherche un ApplicationUser à partir de son Id.
        /// </summary>
        /// <param name="userId">Id de l'ApplicationUser recherché</param>
        /// <returns>l'ApplicationUser trouvé, ou null.</returns>
        public async Task<ApplicationUser> ApplicationUserDeUserId(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Cherche un ApplicationUser à partir de son Email.
        /// </summary>
        /// <param name="eMail">Email de l'ApplicationUser recherché</param>
        /// <returns>l'ApplicationUser trouvé, ou null.</returns>
        public async Task<ApplicationUser> ApplicationUserDeEmail(string eMail)
        {
            return await _userManager.FindByEmailAsync(eMail);
        }

        /// <summary>
        /// Cherche un ApplicationUser à partir de son Email et vérifie son mot de passe.
        /// </summary>
        /// <param name="eMail">email de l'ApplicationUser recherché</param>
        /// <param name="password">mot de passe à vérifier</param>
        /// <returns>l'ApplicationUser trouvé si le mot de passe correspond à l'email, ou null.</returns>
        public async Task<ApplicationUser> ApplicationUserVérifié(string eMail, string password)
        {
            if (!string.IsNullOrEmpty(eMail) && !string.IsNullOrEmpty(password))
            {
                // get the user to verifty
                ApplicationUser user = await _userManager.FindByNameAsync(eMail);

                if (user != null)
                {
                    // check the credentials
                    if (await _userManager.CheckPasswordAsync(user, password))
                    {
                        return await Task.FromResult(user);
                    }
                }
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ApplicationUser>(null);
        }

        /// <summary>
        /// Cherche un Utilisateur à partir de son ApplicationUser.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>l'Utilisateur trouvé qui inclut ses Roles qui incluent leurs Site, ou null.</returns>
        public async Task<Utilisateur> UtilisateurDeApplicationUser(ApplicationUser user)
        {
            return await _context.Utilisateur.Where(utilisateur => utilisateur.UserId == user.Id)
                .Include(utilisateur => utilisateur.Roles)
                .ThenInclude(role => role.Site)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cherche un Utilisateur à partir de son Uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>l'Utilisateur trouvé qui inclut son ApplicationUser, ou null.</returns>
        public async Task<Utilisateur> UtilisateurDeUid(string uid)
        {
            Utilisateur utilisateur = await _context.Utilisateur.FindAsync(uid);
            if (utilisateur != null)
            {
                ApplicationUser applicationUser = await _context.Users.FindAsync(utilisateur.UserId);
                utilisateur.ApplicationUser = applicationUser;
            }
            return utilisateur;
        }

        /// <summary>
        /// Cherche un Role à partir de sa KeyUidRno.
        /// </summary>
        /// <param name="iKeyRole">objet ayant l'Uid et le Rno du Role recherché</param>
        /// <returns>le Role trouvé qui inclut son Site, ou null.</returns>
        public async Task<Role> RoleDeKey(IKeyUidRno iKeyRole)
        {
            Role role = await _context.Role
                .Where(r => r.Uid == iKeyRole.Uid && r.Rno == iKeyRole.Rno)
                .Include(r => r.Site)
                .FirstOrDefaultAsync();
            return role;
        }

        #endregion // Recherche

        #region CarteUtilisateur

        /// <summary>
        /// Crée une CarteUtilisateur à partir d'un ApplicationUser.
        /// Fixe l'Utilisateur de la carte avec son ApplicationUser et ses Roles incluant leurs Sites et leurs Archives.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>la CarteUtilisateur créée</returns>
        public async Task<CarteUtilisateur> CréeCarteUtilisateur(ApplicationUser user)
        {
            Utilisateur utilisateur = await _context.Utilisateur.Where(u => u.UserId == user.Id)
                .Include(u => u.Roles).ThenInclude(r => r.Site)
                .Include(u => u.Roles).ThenInclude(r => r.Archives)
                .FirstAsync();
            utilisateur.ApplicationUser = user;

            CarteUtilisateur carte = new CarteUtilisateur(_context);
            await carte.FixeUtilisateur(utilisateur);
            return carte;
        }

        /// <summary>
        /// Crée une CarteUtilisateur à partir des Claims envoyées avec une requête Http.
        /// Fixe l'Utilisateur de la carte avec son ApplicationUser et ses Roles incluant leurs Sites.
        /// </summary>
        /// <param name="httpContext">le HttpContext de la requête</param>
        /// <returns>la CarteUtilisateur créée, ou null si les Claims ne sont pas valide</returns>
        public async Task<CarteUtilisateur> CréeCarteUtilisateur(HttpContext httpContext)
        {
            CarteUtilisateur carte = new CarteUtilisateur(_context);
            ClaimsPrincipal user = httpContext.User;
            IEnumerable<Claim> claims = user.Identities.FirstOrDefault()?.Claims;
            if (claims == null || claims.Count() == 0)
            {
                return carte;
            }
            string userId = claims.Where(c => c.Type == JwtClaims.UserId).First()?.Value;
            string userName = claims.Where(c => c.Type == JwtClaims.UserName).First()?.Value;
            string uid = claims.Where(c => c.Type == JwtClaims.UtilisateurId).First()?.Value;
            int sessionId = int.Parse(claims.Where(c => c.Type == JwtClaims.SessionId).First()?.Value);

            Utilisateur utilisateur = await _context.Utilisateur
                .Include(u => u.Roles).ThenInclude(r => r.Site)
                .Where(u => u.UserId != null && u.UserId == userId && u.Uid == uid)
                .FirstOrDefaultAsync();
            if (utilisateur == null)
            {
                return carte;
            }
            ApplicationUser applicationUser = utilisateur.ApplicationUser;
            if (userName != applicationUser.UserName)
            {
                // fausse carte
                return carte;
            }

            await carte.FixeUtilisateur(utilisateur);
            carte.SessionId = sessionId;

            return carte;
        }

        /// <summary>
        /// Ajoute un header contenant le jeton identifiant de la carte à la response d'une requête de connection
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="carte"></param>
        /// <returns></returns>
        public async Task AjouteCarteAResponse(HttpResponse httpResponse, CarteUtilisateur carte)
        {
            JwtRéponse jwtRéponse = await _jwtFabrique.CréeReponse(carte);
            string header = JsonSerializer.Serialize(jwtRéponse);
            httpResponse.Headers.Add(JwtFabrique.NomJwtRéponse, header);
        }

        #endregion // CarteUtilisateur

        #region Ajout Suppression

        /// <summary>
        /// Ajoute à la bdd un nouvel Utilisateur et fixe son UserId si l'ApplicationUser paramétre n'est pas null.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<RetourDeService<Utilisateur>> CréeUtilisateur(ApplicationUser user)
        {
            // recherche la première valeur libre dans la liste dans l'ordre croissant des valeurs numériques des Uid des utilisateurs existants
            string[] uids = await _context.Utilisateur.Select(u => u.Uid).ToArrayAsync();
            int[] valeurs = uids.Select(u => int.Parse(u)).OrderBy(l => l).ToArray();
            int nb = valeurs.Length;
            // si la première valeur (d'index 1 - 1) est 1, 1 n'est pas libre
            // si la première valeur (d'index 1 - 1) n'est pas 1, 1 est libre
            // si la valeur suivante (d'index 2 - 1) est 2, 2 n'est pas libre
            // si la valeur suivante (d'index 2 - 1) n'est pas 2, 2 est libre
            // etc.
            // la première valeur libre suit la dernière valeur d'index égal à valeur - 1
            int valeur = 1;
            for (; valeur <= nb && valeur == valeurs[valeur - 1]; valeur++)
            {
            }
            string uid = valeur.ToString();

            Utilisateur utilisateur = new Utilisateur
            {
                Uid = uid,
                Etat = TypeEtatUtilisateur.Nouveau,
            };
            if (user != null)
            {
                utilisateur.UserId = user.Id;
            }
            ArchiveUtilisateur archive = new ArchiveUtilisateur
            {
                Uid = uid,
                Etat = TypeEtatUtilisateur.Nouveau,
                Date = DateTime.Now
            };
            _context.Utilisateur.Add(utilisateur);
            _context.ArchiveUtilisateur.Add(archive);
            return await SaveChangesAsync(utilisateur);
        }

        /// <summary>
        /// Ajoute à la bdd un nouvel Utilisateur sans ApplicationUser.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<RetourDeService<Utilisateur>> CréeUtilisateur()
        {
            return await CréeUtilisateur((ApplicationUser)null);
        }

        /// <summary>
        /// Ajoute à la bdd un nouvel Utilisateur et son ApplicationUser à partir de son Email et de son mot de passe.
        /// </summary>
        /// <param name="vue">objet ayant l'Email et le Password de l'utilisateur à créer</param>
        /// <returns></returns>
        public async Task<RetourDeService<ApplicationUser>> CréeUtilisateur(ICréeCompteVue vue)
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = vue.Email,
                Email = vue.Email,
            };
            try
            {
                var identityResult = await _userManager.CreateAsync(applicationUser, vue.Password);
                if (!identityResult.Succeeded)
                {
                    return new RetourDeService<ApplicationUser>(identityResult);
                }

                RetourDeService<Utilisateur> retour = await CréeUtilisateur(applicationUser);
                if (!retour.Ok)
                {
                    return new RetourDeService<ApplicationUser>(retour);
                }

                applicationUser.Utilisateur = retour.Entité;
                return new RetourDeService<ApplicationUser>(applicationUser);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new RetourDeService<ApplicationUser>(TypeRetourDeService.ConcurrencyError);
            }
            catch (Exception)
            {
                return new RetourDeService<ApplicationUser>(TypeRetourDeService.Indéterminé);
            }
        }

        /// <summary>
        /// Supprime dans la bdd un Utilisateur et son ApplicationUser s'il en a un.
        /// </summary>
        /// <param name="utilisateur">Utilisateur</param>
        /// <returns></returns>
        public async Task<RetourDeService> Supprime(Utilisateur utilisateur)
        {
            string userId = utilisateur.UserId;
            _context.Utilisateur.Remove(utilisateur);
            RetourDeService retour = await SaveChangesAsync();
            if (!retour.Ok || userId == null)
            {
                return retour;
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(applicationUser);
            _context.Remove(applicationUser);
            return await SaveChangesAsync();
        }

        #endregion // Ajout Suppression

        #region Compte

        public async Task EnvoieEmailConfirmeCompte(ApplicationUser user)
        {
            string objet = "Confirmation de votre compte " + ClientApp.Nom;
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeEmail);
            Dictionary<string, string> urlParams = new Dictionary<string, string>
            {
                { "email", user.Email }
            };

            string message = "Veuillez confirmer votre compte";

            await _emailService.EnvoieEmail(user.Email, objet, message, urlBase, token, urlParams);
        }

        /// <summary>
        /// Retourne vrai si la confimation de l'email a eu lieu
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<bool> EmailConfirmé(ApplicationUser user, string code)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task ConfirmeEmailDirect(ApplicationUser user)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

        }

        public async Task EnvoieEmailRéinitialiseMotDePasse(ApplicationUser user)
        {
            string objet = "Mot de passe oublié";
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.RéinitialiseMotDePasse);
            Dictionary<string, string> urlParams = new Dictionary<string, string>
            {
                { "id", user.Id }
            };
            string message = "Vous pourrez mettre à jour votre mot de passe";

            await _emailService.EnvoieEmail(user.Email, objet, message, urlBase, token, urlParams);
        }

        public async Task<bool> RéinitialiseMotDePasse(ApplicationUser user, string code, string motDePasse)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, motDePasse);
            return result.Succeeded;
        }

        public async Task<bool> ChangeMotDePasse(ApplicationUser user, string motDePasse, string nouveauMotDePasse)
        {
            IdentityResult result = await _userManager.ChangePasswordAsync(user, motDePasse, nouveauMotDePasse);
            return result.Succeeded;
        }

        public async Task EnvoieEmailChangeEmail(ApplicationUser user, string nouvelEmail)
        {
            string objet = "Changement d'adresse email";
            string token = await _userManager.GenerateChangeEmailTokenAsync(user, nouvelEmail);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeChangeEmail);
            Dictionary<string, string> urlParams = new Dictionary<string, string>
            {
                { "id", user.Id },
                { "email", user.Email }
            };
            string message = "Le changement de votre adresse mail sera confirmé";

            await _emailService.EnvoieEmail(nouvelEmail, objet, message, urlBase, token, urlParams);
        }

        public async Task<bool> ChangeEmail(ApplicationUser user, string nouvelEmail, string code)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ChangeEmailAsync(user, nouvelEmail, token);
            return result.Succeeded;
        }

        #endregion // Compte

        /// <summary>
        /// Trouve l'invitation enregistrée ayant le même email
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<Invitation> TrouveInvitation(IInvitationKey key)
        {
            return await _context.Invitation
                .Where(i => i.Email == key.Email && i.Uid == key.Uid && i.Rno == key.Rno)
                .FirstOrDefaultAsync();
        }

        public async Task<RetourDeService<Invitation>> EnvoieEmailDevenirClient(Invitation invitation, InvitationVérifiée vérifiée)
        {
            string objet = "Devenir client de " + vérifiée.Site.Titre;
            string urlBase = ClientApp.Url(ClientApp.DevenirClient);
            string message = "Vous pouvez devenir client de " + vérifiée.Site.Titre;

            await _emailService.EnvoieEmail<Invitation>(invitation.Email, objet, message, urlBase, invitation, null);

            if (vérifiée.Invitation != null)
            {
                _context.Invitation.Remove(vérifiée.Invitation);
                RetourDeService<Invitation> retourSupprime = await SaveChangesAsync(vérifiée.Invitation);
                if (!retourSupprime.Ok)
                {
                    return retourSupprime;
                }
            }
            _context.Invitation.Add(invitation);
            return await SaveChangesAsync(invitation);
        }

        public Invitation DécodeInvitation(string code)
        {
            return _emailService.DécodeCodeDeEmail<Invitation>(code);
        }

        public async Task Connecte(CarteUtilisateur carteUtilisateur)
        {
            Utilisateur utilisateur = carteUtilisateur.Utilisateur;
            // persistant est false car l'utilisateur doit s'authentifier à chaque accès
            await _signInManager.SignInAsync(utilisateur.ApplicationUser, false);
            // lit et augmente le sessionId de l'utilisateur
            int sessionId = utilisateur.SessionId;
            if (sessionId < 0)
            {
                // l'utilisateur s'est déconnecté lors de sa dernière session
                // et son SessionId a été changé en son opposé
                sessionId = -sessionId;
            }
            sessionId = sessionId + 1;
            // fixe le sessionId de l'utilisateur et de la carte
            utilisateur.SessionId = sessionId;
            _context.Utilisateur.Update(utilisateur);
            ArchiveUtilisateur archive = new ArchiveUtilisateur
            {
                Uid = utilisateur.Uid,
                Date = DateTime.Now,
                SessionId = sessionId
            };
            _context.ArchiveUtilisateur.Add(archive);
            await _context.SaveChangesAsync();
            carteUtilisateur.SessionId = sessionId;
        }

        public async Task Déconnecte(CarteUtilisateur carteUtilisateur)
        {
            Utilisateur utilisateur = carteUtilisateur.Utilisateur;
            await _signInManager.SignOutAsync();
            // change le sessionId de l'utilisateur en son opposé
            utilisateur.SessionId = -utilisateur.SessionId;
            _context.Utilisateur.Update(utilisateur);
            ArchiveUtilisateur archive = new ArchiveUtilisateur
            {
                Uid = utilisateur.Uid,
                Date = DateTime.Now,
                SessionId = utilisateur.SessionId,
                NoDernierRole = carteUtilisateur.NoDernierRole
            };
            _context.ArchiveUtilisateur.Add(archive);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InvitationVue>> Invitations(AKeyUidRno keySite)
        {
            List<Invitation> invitations = await _context.Invitation
                .Where(i => i.Uid == keySite.Uid && i.Rno == keySite.Rno)
                .ToListAsync();
            return invitations
                .Select(i => new InvitationVue(i))
                .ToList();
        }

        public async Task<RetourDeService<Invitation>> SupprimeInvitation(Invitation invitation)
        {
            _context.Invitation.Remove(invitation);
            return await SaveChangesAsync(invitation);
        }

        public async Task<List<Utilisateur>> Lit()
        {
            return await _context.Utilisateur
                .Include(u => u.ApplicationUser)
                .ToListAsync();
        }

    }

}
