using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Sites;
using System.Security.Claims;
using System.Text;
using KalosfideAPI.Roles;
using KalosfideAPI.Clients;
using System.Text.Json;

namespace KalosfideAPI.Utilisateurs
{

    public class UtilisateurService : BaseService<Utilisateur>, IUtilisateurService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoleService _roleService;
        protected readonly ISiteService _siteService;
        private readonly IClientService _clientService;
        private readonly IDataProtector _protector;

        public UtilisateurService(
            ApplicationContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ISiteService siteService,
            IRoleService roleService,
            IClientService clientService,
            IDataProtectionProvider dataProtectionProvider
            ) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _siteService = siteService;
            _roleService = roleService;
            _clientService = clientService;
            _protector = dataProtectionProvider.CreateProtector("Kalosfide.devenir.client");
        }

        #region Validation

        public async Task<bool> NomUnique(string nom)
        {
            var doublon = await _context.Users.Where(s => s.UserName == nom).FirstOrDefaultAsync();
            return doublon == null;
        }

        #endregion // Validation

        public async Task<ApplicationUser> TrouveParId(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> TrouveParNom(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<ApplicationUser> TrouveParEmail(string eMail)
        {
            return await _userManager.FindByEmailAsync(eMail);
        }

        public async Task<ApplicationUser> ApplicationUserVérifié(string userName, string password)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                // get the user to verifty
                ApplicationUser user = await _userManager.FindByNameAsync(userName);

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

#pragma warning disable IDE0060 // Supprimer le paramètre inutilisé
        private Task SendEmailAsync(string adresse, string objet, string corps)
#pragma warning restore IDE0060 // Supprimer le paramètre inutilisé
        {
            return Task.CompletedTask;
        }

        public async Task EnvoieEmailConfirmeCompte(ApplicationUser user)
        {
            string objet = "Confirmation de votre compte " + ClientApp.Nom;
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string url = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeEmail) + "?id=" + user.Id + "&code=" + code + "&email=" + user.Email;
            string message = "Veuillez confirmer votre compte en cliquant sur ce lien: <a href=\"" + url + "\">" + url + "</a>";

            await SendEmailAsync(user.Email, objet, message);
        }

        public async Task EnvoieEmailRéinitialiseMotDePasse(ApplicationUser user)
        {
            string objet = "Mot de passe oublié";
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string url = ClientApp.Url(ClientApp.Compte, ClientApp.RéinitialiseMotDePasse) + "?id=" + user.Id + "&code=" + code;
            string message = "Vous pourrez mettre à jour votre mot de passe en cliquant sur ce lien: <a href=\"" + url + "\">" + url + "</a>";

            await SendEmailAsync(user.Email, objet, message);
        }

        public async Task EnvoieEmailChangeEmail(ApplicationUser user, string nouvelEmail)
        {
            string objet = "Changement d'adresse email";
            string token = await _userManager.GenerateChangeEmailTokenAsync(user, nouvelEmail);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string url = ClientApp.Url(ClientApp.Compte, ClientApp.ConfirmeChangeEmail) + "?id=" + user.Id + "&code=" + code + "&email=" + nouvelEmail;
            string message = "Le changement de votre adresse mail sera confirmé en cliquant sur ce lien: <a href=\"" + url + "\">" + url + "</a>";

            await SendEmailAsync(user.Email, objet, message);
        }


        public async Task<Invitation> TrouveInvitation(IInvitationKey key)
        {
            return await _context.Invitation
                .Where(i => i.Email == key.Email && i.Uid == key.Uid && i.Rno == key.Rno)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Trouve le client défini par la clé. Retourne null si le client n'existe pas ou n'appartient pas au site ou a déjà un compte
        /// </summary>
        /// <param name="site"></param>
        /// <param name="uidClient"></param>
        /// <param name="rnoClient"></param>
        /// <returns></returns>
        public async Task<Client> TrouveClientDansSite(Site site, string uidClient, int rnoClient)
        {
            Client client = await _context.Client
                .Include(c => c.Role)
                .Where(c => c.Uid == uidClient && c.Rno == rnoClient && c.Role.SiteUid == site.Uid && c.Role.SiteRno == site.Rno)
                .FirstOrDefaultAsync();
            return client;
        }

        public async Task EnvoieEmailDevenirClient(Invitation invitation, InvitationVérifiée vérifiée)
        {
            string objet = "Devenir client de " + vérifiée.Site.Titre;
            string texte = JsonSerializer.Serialize(invitation);
            string token = _protector.Protect(texte); // Cryptage.Encrypte();
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string url = ClientApp.Url(ClientApp.DevenirClient) + "?code=" + code;
            string message = "Vous pouvez devenir client de " + vérifiée.Site.Titre + " en cliquant sur ce lien: <a href=\"" + url + "\">" + url + "</a>";

            await SendEmailAsync(invitation.Email, objet, message);
        }

        public async Task<RetourDeService<Invitation>> RemplaceInvitation(Invitation enregistrée, Invitation invitation)
        {
            if (enregistrée != null)
            {
                _context.Invitation.Remove(enregistrée);
                RetourDeService<Invitation> retourSupprime = await SaveChangesAsync(enregistrée);
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
            byte[] décodé = WebEncoders.Base64UrlDecode(code);
            string token = Encoding.UTF8.GetString(décodé);
            Invitation invitation;
            try
            {
                string x = _protector.Unprotect(token);
                invitation = JsonSerializer.Deserialize<Invitation>(x); // Cryptage.Décrypte<Invitation>(token);
            }
            catch (Exception)
            {
                invitation = null;
            }
            return invitation;
        }

        public async Task<Client> ClientInvité(string uid, int rno)
        {
            Client client = await _context.Client
                .Include(c => c.Role)
                .Where(c => c.Uid == uid && c.Rno == rno)
                .FirstOrDefaultAsync();
            return client;
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

        public async Task<bool> ChangeEmail(ApplicationUser user, string nouvelEmail, string code)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ChangeEmailAsync(user, nouvelEmail, token);
            return result.Succeeded;
        }

        public async Task Connecte(ApplicationUser user, bool persistant)
        {
            await _signInManager.SignInAsync(user, persistant);
        }

        public async Task Déconnecte()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<CarteUtilisateur> CréeCarteUtilisateur(ApplicationUser user)
        {
            Utilisateur utilisateur = await _context.Utilisateur.Where(u => u.UserId == user.Id)
                .Include(u => u.Roles).ThenInclude(r => r.Site)
                .FirstOrDefaultAsync();

            CarteUtilisateur carte = new CarteUtilisateur(_context);
            await carte.FixeUtilisateur(utilisateur);
            return carte;
        }

        public async Task<CarteUtilisateur> CréeCarteUtilisateur(ClaimsPrincipal user)
        {
            IEnumerable<Claim> claims = user.Identities.FirstOrDefault()?.Claims;
            if (claims == null || claims.Count() == 0)
            {
                return null;
            }
            string userId = claims.Where(c => c.Type == JwtClaims.UserId).First()?.Value;
            string userName = claims.Where(c => c.Type == JwtClaims.UserName).First()?.Value;
            string uid = claims.Where(c => c.Type == JwtClaims.UtilisateurId).First()?.Value;

            Utilisateur utilisateur = await _context.Utilisateur
                .Include(u => u.Roles).ThenInclude(r => r.Site)
                .Where(u => u.UserId != null && u.UserId == userId && u.Uid == uid)
                .FirstOrDefaultAsync();
            if (utilisateur == null)
            {
                return null;
            }
            ApplicationUser applicationUser = utilisateur.ApplicationUser;
            if (userName != applicationUser.UserName)
            {
                // fausse carte
                return null;
            }
            CarteUtilisateur carte = new CarteUtilisateur(_context);
            await carte.FixeUtilisateur(utilisateur);

            return carte;
        }

        public bool VérifieTrimCréeSiteVue(ICréeSiteVue vue)
        {
            if (vue.Url == null)
            {
                return false;
            }
            vue.Url = vue.Url.Trim();
            if (vue.Url.Length == 0)
            {
                return false;
            }
            if (vue.Titre == null)
            {
                return false;
            }
            vue.Titre = vue.Titre.Trim();
            if (vue.Titre.Length == 0)
            {
                return false;
            }
            if (vue.Nom == null)
            {
                return false;
            }
            vue.Nom = vue.Nom.Trim();
            if (vue.Nom.Length == 0)
            {
                return false;
            }
            if (vue.Adresse == null)
            {
                return false;
            }
            vue.Adresse = vue.Adresse.Trim();
            if (vue.Adresse.Length == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<RetourDeService<Role>> CréeRoleSite(string uid, ICréeSiteVue vue)
        {
            int rno = (await _roleService.DernierNo(uid)) + 1;

            Role role = new Role
            {
                Uid = uid,
                Rno = rno,
                SiteUid = uid,
                SiteRno = rno,
                Etat = TypeEtatRole.Nouveau
            };
            _roleService.AjouteSansSauver(role);
            Site site = new Site
            {
                Uid = uid,
                Rno = rno,
                Url = vue.Url,
                Titre = vue.Titre,
                Nom = vue.Nom,
                Adresse = vue.Adresse,
                Ville = vue.Ville,
                Etat = TypeEtatSite.Catalogue
            };
            _siteService.AjouteSansSauver(site);
            role.Site = site;

            return await SaveChangesAsync(role);
        }

        public bool VérifieTrimDevenirClientVue(DevenirClientVue vue)
        {
            if (vue.Nom == null)
            {
                return false;
            }
            vue.Nom = vue.Nom.Trim();
            if (vue.Nom.Length == 0)
            {
                return false;
            }
            if (vue.Adresse == null)
            {
                return false;
            }
            vue.Adresse = vue.Adresse.Trim();
            if (vue.Adresse.Length == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> VérifieNomPris(Site site, string Nom, Client clientInvité)
        {
            Client client = await _context.Client
                .Include(c => c.Role)
                .Where(c => c.Role.SiteUid == site.Uid && c.Role.SiteRno == site.Rno && c.Nom == Nom)
                .FirstOrDefaultAsync();
            return client != null && (clientInvité == null || clientInvité.Uid != client.Uid || clientInvité.Rno != client.Rno);
        }

        public async Task<RetourDeService> CréeRoleClient(Site site, Utilisateur utilisateur, Client clientInvité, DevenirClientVue vue)
        {
            RetourDeService<Client> retourClient = await _clientService.Ajoute(utilisateur, site, vue);
            if (!retourClient.Ok || clientInvité == null)
            {
                return retourClient;
            }
            string uid = retourClient.Entité.Uid;
            int rno = retourClient.Entité.Rno;

            List<DocCLF> anciensDocs = await _context.Docs
                .Where(d => d.Uid == clientInvité.Uid && d.Rno == clientInvité.Rno)
                .ToListAsync();
            if (anciensDocs.Count == 0)
            {
                return retourClient;
            }
            List<DocCLF> nouveauxDocs = anciensDocs
                .Select(d => DocCLF.Clone(uid, rno, d))
                .ToList();
            List<LigneCLF> anciennesLignes = await _context.Lignes
                .Where(l => l.Uid == clientInvité.Uid && l.Rno == clientInvité.Rno)
                .ToListAsync();
            if (anciennesLignes.Count == 0)
            {
                return retourClient;
            }
            List<LigneCLF> nouvellesLignes = anciennesLignes
                .Select(l => LigneCLF.Clone(uid, rno, l))
                .ToList();
            _context.Docs.AddRange(nouveauxDocs);
            var r = await SaveChangesAsync();
            if (r.Ok)
            {
                _context.Lignes.AddRange(nouvellesLignes);
                r = await SaveChangesAsync();
            }
            await _clientService.SupprimeSansSauver(clientInvité);
            _context.Role.Remove(clientInvité.Role);
            await SaveChangesAsync();
            return retourClient;
        }

        public async Task<List<InvitationVue>> Invitations(Site site)
        {
            List<Invitation> invitations = await _context.Invitation
                .Where(i => i.Uid == site.Uid && i.Rno == site.Rno)
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

                Utilisateur utilisateur = await CréeUtilisateurSansSauver();
                utilisateur.UserId = applicationUser.Id;

                await _context.SaveChangesAsync();
                applicationUser.Utilisateur = utilisateur;
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

        public async Task<RetourDeService<Utilisateur>> CréeUtilisateur(ApplicationUser applicationUser, string password)
        {
            try
            {
                var identityResult = await _userManager.CreateAsync(applicationUser, password);
                if (!identityResult.Succeeded)
                {
                    return new RetourDeService<Utilisateur>(identityResult);
                }

                Utilisateur utilisateur = await CréeUtilisateurSansSauver();
                utilisateur.UserId = applicationUser.Id;

                await _context.SaveChangesAsync();
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

        public async Task<RetourDeService<Utilisateur>> CréeUtilisateur()
        {
            Utilisateur utilisateur = await CréeUtilisateurSansSauver();
            await _context.SaveChangesAsync();
            return new RetourDeService<Utilisateur>(utilisateur);
        }

        public async Task<Utilisateur> CréeUtilisateurSansSauver()
        {
            List<long> x = await _context.Utilisateur.Select(u => long.Parse(u.Uid)).ToListAsync();
            long Max = x.Count == 0 ? 1 : x.Max() + 1;

            Utilisateur utilisateur = new Utilisateur
            {
                Uid = Max.ToString(),
                Etat = TypeEtatUtilisateur.Nouveau,
            };
            _context.Utilisateur.Add(utilisateur);
            ArchiveUtilisateur changement = new ArchiveUtilisateur
            {
                Uid = utilisateur.Uid,
                Etat = TypeEtatUtilisateur.Nouveau,
                Date = DateTime.Now
            };
            _context.ArchiveUtilisateur.Add(changement);
            return utilisateur;
        }

        public async Task<Utilisateur> Lit(string id)
        {
            Utilisateur utilisateur = await _context.Utilisateur.FindAsync(id);
            if (utilisateur != null)
            {
                ApplicationUser applicationUser = await _context.Users.FindAsync(utilisateur.UserId);
                utilisateur.ApplicationUser = applicationUser;
            }
            return utilisateur;
        }

        public async Task<Utilisateur> UtilisateurDeUser(string userId)
        {
            return await _context.Utilisateur.Where(utilisateur => utilisateur.UserId == userId)
                .Include(utilisateur => utilisateur.Roles)
                .ThenInclude(role => role.Site)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Utilisateur>> Lit()
        {
            return await _context.Utilisateur
                .Include(u => u.ApplicationUser)
                .ToListAsync();
        }

        public async Task<RetourDeService<Utilisateur>> Supprime(Utilisateur utilisateur)
        {
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(utilisateur.UserId);
            await _userManager.DeleteAsync(applicationUser);
            _context.Remove(applicationUser);
            return await SaveChangesAsync(utilisateur);
        }

        public async Task<RetourDeService<Utilisateur>> ChangeEtat(Utilisateur utilisateur, string état)
        {
            utilisateur.Etat = état;
            _context.Utilisateur.Update(utilisateur);
            ArchiveUtilisateur changement = new ArchiveUtilisateur
            {
                Uid = utilisateur.Uid,
                Date = DateTime.Now,
                Etat = état
            };
            _context.ArchiveUtilisateur.Add(changement);
            try
            {
                await _context.SaveChangesAsync();
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

        public async Task<bool> UserNamePris(string userName)
        {
            return await _context.Users.Where(user => user.UserName == userName).AnyAsync();
        }

        public async Task<bool> EmailPris(string eMail)
        {
            return await _context.Users.Where(user => user.Email == eMail).AnyAsync();
        }
    }

}
