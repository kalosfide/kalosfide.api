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

    public class UtilisateurService : BaseService, IUtilisateurService
    {
        private readonly SignInManager<Utilisateur> _signInManager;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly IJwtFabrique _jwtFabrique;
        private readonly IEnvoieEmailService _emailService;

        public UtilisateurService(
            ApplicationContext context,
            UserManager<Utilisateur> userManager,
            SignInManager<Utilisateur> signInManager,
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
        /// Cherche un Utilisateur à partir de son Id.
        /// </summary>
        /// <param name="id">Id de l'Utilisateur recherché</param>
        /// <returns>l'Utilisateur trouvé, ou null.</returns>
        public async Task<Utilisateur> UtilisateurDeId(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email.
        /// </summary>
        /// <param name="eMail">Email de l'Utilisateur recherché</param>
        /// <returns>l'Utilisateur trouvé, ou null.</returns>
        public async Task<Utilisateur> UtilisateurDeEmail(string eMail)
        {
            Utilisateur utilisateur = await _userManager.FindByEmailAsync(eMail);
            if (utilisateur != null)
            {
                Utilisateur avecRoles = await _context.Utilisateur
                    .Where(u => u.Id == utilisateur.Id)
                    .Include(u => u.Fournisseurs)
                    .Include(u => u.Clients)
                    .FirstAsync();
            }
            return utilisateur;
        }

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email et vérifie son mot de passe.
        /// </summary>
        /// <param name="eMail">email de l'Utilisateur recherché</param>
        /// <param name="password">mot de passe à vérifier</param>
        /// <returns>l'Utilisateur trouvé si le mot de passe correspond à l'email, ou null.</returns>
        public async Task<Utilisateur> UtilisateurVérifié(string eMail, string password)
        {
            if (!string.IsNullOrEmpty(eMail) && !string.IsNullOrEmpty(password))
            {
                // get the user to verifty
                Utilisateur user = await _userManager.FindByNameAsync(eMail);

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
            return await Task.FromResult<Utilisateur>(null);
        }

        /// <summary>
        /// Trouve dans la bdd l'Utilisateur incluant ses Fournisseurs et ses Clients correspondant à un Utilisateur retourné par le UserManager.
        /// </summary>
        /// <param name="user">Utilisateur retourné par le UserManager</param>
        /// <returns>l'Utilisateur trouvé qui inclut ses Fournisseurs et ses Clients.</returns>
        public async Task<Utilisateur> UtilisateurAvecRoles(Utilisateur user)
        {
            return await _context.Utilisateur.Where(utilisateur => utilisateur.Id == user.Id)
                .Include(utilisateur => utilisateur.Fournisseurs)
                .Include(utilisateur => utilisateur.Clients)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cherche un Utilisateur à partir de son Email et, s'il existe, vérifie s'il est usager d'un Site.
        /// </summary>
        /// <param name="email">Email de l'Utilisateur à chercher</param>
        /// <param name="idSite">Id d'un Site</param>
        /// <returns>true, si l'Utilisateur existe et est usager du Site; false, sinon.</returns>
        public async Task<bool> UtilisateurDeEmailEstUsagerDeSite(string email, uint idSite)
        {
            Utilisateur utilisateur = await _context.Utilisateur.Where(utilisateur => utilisateur.Email == email)
                .Include(utilisateur => utilisateur.Fournisseurs)
                .Include(utilisateur => utilisateur.Clients)
                .FirstOrDefaultAsync();
            if (utilisateur == null)
            {
                return false;
            }
            return Utilisateur.EstUsager(utilisateur, idSite);
        }

        #endregion // Recherche

        #region CarteUtilisateur

        /// <summary>
        /// Crée une CarteUtilisateur à partir d'un Utilisateur.
        /// Fixe l'Utilisateur de la carte avec son Archives, avec ses Clients incluant leurs Archives et leur Site incluant son Fournisseur
        /// et avec ses Fournisseurs incluant leurs Archives et leur Site.
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns>la CarteUtilisateur créée</returns>
        public async Task<CarteUtilisateur> CréeCarteUtilisateur(Utilisateur utilisateur)
        {

            CarteUtilisateur carte = new CarteUtilisateur(_context);
            await carte.FixeUtilisateur(utilisateur);
            return carte;
        }

        /// <summary>
        /// Crée une CarteUtilisateur à partir des Claims envoyées avec une requête Http.
        /// Fixe l'Utilisateur de la carte avec son Archives, avec ses Clients incluant leurs Archives et leur Site incluant son Fournisseur
        /// et avec ses Fournisseurs incluant leurs Archives et leur Site.
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
            int sessionId = int.Parse(claims.Where(c => c.Type == JwtClaims.SessionId).First()?.Value);

            Utilisateur utilisateur = await _context.Utilisateur
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            if (utilisateur == null)
            {
                return carte;
            }
            if (userName != utilisateur.UserName)
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
        /// Ajoute à la bdd un nouvel Utilisateur à partir de son Email et de son mot de passe.
        /// </summary>
        /// <param name="vue">objet ayant l'Email et le Password de l'utilisateur à créer</param>
        /// <returns></returns>
        public async Task<RetourDeService<Utilisateur>> CréeUtilisateur(ICréeCompteVue vue)
        {
            Utilisateur utilisateur = new Utilisateur
            {
                UserName = vue.Email,
                Email = vue.Email,
                Etat = EtatUtilisateur.Nouveau
            };
            try
            {
                IdentityResult identityResult = await _userManager.CreateAsync(utilisateur, vue.Password);
                if (!identityResult.Succeeded)
                {
                    return new RetourDeService<Utilisateur>(identityResult);
                }

                ArchiveUtilisateur archive = new ArchiveUtilisateur
                {
                    Id = utilisateur.Id,
                    Email = utilisateur.Email,
                    Etat = EtatUtilisateur.Nouveau,
                    Date = DateTime.Now
                };
                _context.ArchiveUtilisateur.Add(archive);
                var retour = await SaveChangesAsync(utilisateur);

                return new RetourDeService<Utilisateur>(utilisateur);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new RetourDeService<Utilisateur>(TypeRetourDeService.ConcurrencyError);
            }
            catch (Exception)
            {
                return new RetourDeService<Utilisateur>(TypeRetourDeService.Indéterminé);
            }
        }

        /// <summary>
        /// Supprime dans la bdd un Utilisateur
        /// </summary>
        /// <param name="utilisateur">Utilisateur</param>
        /// <returns></returns>
        public async Task<RetourDeService> Supprime(Utilisateur utilisateur)
        {

            Utilisateur applicationUser = await _userManager.FindByIdAsync(utilisateur.Id);
            await _userManager.DeleteAsync(applicationUser);
            _context.Remove(applicationUser);
            return await SaveChangesAsync();
        }

        #endregion // Ajout Suppression

        #region Compte

        public TokenDaté DécodeTokenDaté(string code)
        {
            return _emailService.DécodeCodeDeEmail<TokenDaté>(code);
        }

        public async Task EnvoieEmailConfirmeCompte(Utilisateur user)
        {
            string objet = "Confirmation de votre compte " + ClientApp.Nom;
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeEmail);
            List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( "email", user.Email )
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
        public async Task<bool> EmailConfirmé(Utilisateur user, string code)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task ConfirmeEmailDirect(Utilisateur user)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

        }

        /// <summary>
        /// Durée de validité d'un PasswordResetToken généré par l'UserManager.
        /// </summary>
        public TimeSpan DuréeValiditéTokenRéinitialiseMotDePasse
        {
            get
            {
                return new TimeSpan(0, 2, 0);
            }
        }

        /// <summary>
        /// Envoie un email avec un lien ayant en QueryParams l'Id de l'utilisateur et un code représentant le cryptage
        /// d'un TokenDaté contenant un PasswordResetToken généré par l'UserManager et la date d'envoi.
        /// </summary>
        /// <param name="utilisateur">Utilisateur dont on veut réinitialiser le mot de passe</param>
        /// <returns></returns>
        public async Task EnvoieEmailRéinitialiseMotDePasse(Utilisateur utilisateur)
        {
            string objet = "Mot de passe oublié";
            string token = await _userManager.GeneratePasswordResetTokenAsync(utilisateur);
            TokenDaté tokenDaté = new TokenDaté
            {
                Token = token,
                Date = DateTime.Now
            };
            DateTime finValidité = tokenDaté.Date.Add(DuréeValiditéTokenRéinitialiseMotDePasse);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.RéinitialiseMotDePasse);
            List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( "id", utilisateur.Id )
            };
            string message = "Vous pourrez mettre à jour votre mot de passe";

            await _emailService.EnvoieEmail(utilisateur.Email, objet, message, urlBase, tokenDaté, finValidité, urlParams);
        }

        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur.
        /// </summary>
        /// <param name="id">Id de Utilisateur dont on veut réinitialiser le mot de passe</param>
        /// <param name="token">PasswordResetToken généré par l'UserManager</param>
        /// <param name="motDePasse"></param>
        /// <returns>true, si le mot de passe a été changé; false, sinon.</returns>
        public async Task<bool> RéinitialiseMotDePasse(string id, string token, string motDePasse)
        {
            Utilisateur utilisateur = await UtilisateurDeId(id);
            if (utilisateur == null)
            {
                return false;
            }
            IdentityResult result = await _userManager.ResetPasswordAsync(utilisateur, token, motDePasse);
            return result.Succeeded;
        }

        public async Task<bool> ChangeMotDePasse(Utilisateur user, string motDePasse, string nouveauMotDePasse)
        {
            IdentityResult result = await _userManager.ChangePasswordAsync(user, motDePasse, nouveauMotDePasse);
            return result.Succeeded;
        }

        /// <summary>
        /// Durée de validité d'un EmailToken généré par l'UserManager.
        /// </summary>
        public TimeSpan DuréeValiditéTokenChangeEmail
        {
            get
            {
                return new TimeSpan(0, 2, 0);
            }
        }

        /// <summary>
        /// Envoie un email avec un lien ayant en QueryParams l'Id et l'Email de l'utilisateur et un code représentant le cryptage
        /// d'un TokenDaté contenant un EmailToken généré par l'UserManager et la date d'envoi.
        /// </summary>
        /// <param name="utilisateur">Utilisateur qui veut changer d'Email</param>
        /// <returns></returns>
        public async Task EnvoieEmailChangeEmail(Utilisateur utilisateur, string nouvelEmail)
        {
            string objet = "Changement d'adresse email";
            string token = await _userManager.GenerateChangeEmailTokenAsync(utilisateur, nouvelEmail);
            TokenDaté tokenDaté = new TokenDaté
            {
                Token = token,
                Date = DateTime.Now
            };
            DateTime finValidité = tokenDaté.Date.Add(DuréeValiditéTokenChangeEmail);
            string urlBase = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeChangeEmail);
            List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("id", utilisateur.Id),
                new KeyValuePair<string, string>("email", nouvelEmail)
            };
            string message = "Le changement de votre adresse mail sera confirmé";

            await _emailService.EnvoieEmail(nouvelEmail, objet, message, urlBase, tokenDaté, finValidité, urlParams);
        }

        /// <summary>
        /// Change l'Email d'un Utilisateur.
        /// </summary>
        /// <param name="id">Id de Utilisateur qui veut changer d'Email</param>
        /// <param name="nouvelEmail"></param>
        /// <param name="token">EmailToken généré par l'UserManager</param>
        /// <returns></returns>
        public async Task<bool> ChangeEmail(string id, string nouvelEmail, string token)
        {
            Utilisateur utilisateur = await UtilisateurDeId(id);
            if (utilisateur == null)
            {
                return false;
            }
            IdentityResult result = await _userManager.ChangeEmailAsync(utilisateur, nouvelEmail, token);
            return result.Succeeded;
        }

        #endregion // Compte

        public async Task<RetourDeService> Connecte(Utilisateur utilisateur)
        {
            // persistant est false car l'utilisateur doit s'authentifier à chaque accès
            await _signInManager.SignInAsync(utilisateur, false);
            // lit et augmente le sessionId de l'utilisateur
            int sessionId = utilisateur.SessionId;
            if (sessionId < 0)
            {
                // l'utilisateur s'est déconnecté lors de sa dernière session
                // et son SessionId a été changé en son opposé
                sessionId = -sessionId;
            }
            sessionId += 1;
            // fixe le sessionId de l'utilisateur et de la carte
            utilisateur.SessionId = sessionId;
            _context.Utilisateur.Update(utilisateur);
            ArchiveUtilisateur archive = new ArchiveUtilisateur
            {
                Id = utilisateur.Id,
                Date = DateTime.Now,
                SessionId = sessionId
            };
            _context.ArchiveUtilisateur.Add(archive);
            return await SaveChangesAsync();
        }

        public async Task Connecte(CarteUtilisateur carteUtilisateur)
        {
            Utilisateur utilisateur = carteUtilisateur.Utilisateur;
            // persistant est false car l'utilisateur doit s'authentifier à chaque accès
            await _signInManager.SignInAsync(utilisateur, false);
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
                Id = utilisateur.Id,
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
                Id = utilisateur.Id,
                Date = DateTime.Now,
                SessionId = utilisateur.SessionId,
            };
            _context.ArchiveUtilisateur.Add(archive);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Utilisateur>> Lit()
        {
            return await _context.Utilisateur
                .ToListAsync();
        }

        public async Task<RetourDeService> FixeIdDernierSite(Utilisateur utilisateur, uint id)
        {
            ArchiveUtilisateur enregistrée = await _context.ArchiveUtilisateur
                .Where(a => a.Id == utilisateur.Id && a.IdDernierSite != null)
                .OrderBy(a => a.Date)
                .LastOrDefaultAsync();
            if (enregistrée != null && enregistrée.IdDernierSite == id)
            {
                return new RetourDeService(TypeRetourDeService.Ok);
            }
            ArchiveUtilisateur archive = new ArchiveUtilisateur
            {
                Id = utilisateur.Id,
                Date = DateTime.Now,
                IdDernierSite = id
            };
            _context.ArchiveUtilisateur.Add(archive);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Fixe l'UtilisateurId et l'Etat actif du Fournisseur et enregistre dans la bdd.
        /// </summary>
        /// <param name="fournisseur">Fournisseur d'une DemandeSite à activer</param>
        /// <param name="utilisateur">Utilisateur affecté à ce Fournisseur</param>
        /// <returns></returns>
        public async Task<RetourDeService> FixeUtilisateur(Fournisseur fournisseur, Utilisateur utilisateur)
        {
            fournisseur.UtilisateurId = utilisateur.Id;
            fournisseur.Etat = EtatRole.Actif;
            _context.Fournisseur.Update(fournisseur);
            return await FixeIdDernierSite(utilisateur, fournisseur.Id);
        }

    }

}
