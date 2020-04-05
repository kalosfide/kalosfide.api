using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Sites;
using System.Security.Claims;

namespace KalosfideAPI.Utilisateurs
{

    public class UtilisateurService : BaseService<Utilisateur>, IUtilisateurService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UtilisateurService(
            ApplicationContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            ) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            return await _context.Users.FindAsync(userId);
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
                string userId = (claims.Where(c => c.Type == JwtClaims.UserId).First())?.Value;
                string userName = (claims.Where(c => c.Type == JwtClaims.UserName).First())?.Value;
                string uid = (claims.Where(c => c.Type == JwtClaims.UtilisateurId).First())?.Value;

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

        // un utilisateur ne peut pas devenir client d'un site où il est déjà client avec le même nom
        // inutile si nom client unique sur le site
        public async Task<bool> PeutAjouterRole(Utilisateur utilisateur, EnregistrementClientVue client)
        {
            var existe = await _context.Role.Where(role => role.Uid == utilisateur.Uid)
                .Join(_context.Client, role => new { role.Uid, role.Rno }, client1 => new { client1.Uid, client1.Rno }, (role, client1) => new { role, client1 })
                .Where(rc => rc.client1.Nom == client.Nom && rc.role.Uid == client.SiteUid && rc.role.Rno == client.SiteRno).AnyAsync();
            return !existe;
        }

        // un utilisateur ne peut pas devenir fournisseur d'un site où il est déjà client avec le même nom 
        public async Task<bool> PeutAjouterRole(Utilisateur utilisateur, EnregistrementFournisseurVue fournisseur)
        {
            var existe = await _context.Role.Where(role => role.Uid == utilisateur.Uid)
                .Join(_context.Client,
                    role => new { role.Uid, role.Rno },
                    fournisseur1 => new { fournisseur1.Uid, fournisseur1.Rno },
                    (role, fournisseur1) => fournisseur1)
                .Where(fournisseur1 => fournisseur1.Nom == fournisseur.Nom).AnyAsync();
            return !existe;
        }

        public async Task<RetourDeService<Utilisateur>> CréeUtilisateur(ApplicationUser applicationUser, string password)
        {
            try
            {
                var identityResult = await _userManager.CreateAsync(applicationUser, password);
                if (!identityResult.Succeeded)
                {
                    return new RetourDeService<Utilisateur>(TypeRetourDeService.IdentityError);
                }
                await _context.SaveChangesAsync();

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
            long Max;
            if (await _context.Utilisateur.AnyAsync())
            {
                Max = await _context.Utilisateur.MaxAsync(u => long.Parse(u.Uid)) + 1;
            }
            else
            {
                Max = 1;
            }
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
            return await _context.Utilisateur.Where(utilisateur => utilisateur.UserId == userId).FirstOrDefaultAsync();
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
