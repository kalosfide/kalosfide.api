using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{
    public class AdminService: BaseService, IAdminService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly ISiteService _siteService;

        public AdminService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            ISiteService siteService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _siteService = siteService;
        }

        public async Task<List<Fournisseur>> Fournisseurs()
        {
            List<Role> roles = await _context.Role
                .Where(role => role.Uid == role.SiteUid && role.Rno == role.SiteRno)
                .Include(role => role.Utilisateur).ThenInclude(u => u.ApplicationUser)
                .Include(role => role.Site)
                .Include(role => role.Archives)
                .AsNoTracking()
                .ToListAsync();
            List<Fournisseur> fournisseurs = roles.Select(role => new Fournisseur(role)).ToList();
            return fournisseurs;
        }

        public async Task<Fournisseur> Fournisseur(KeyUidRno keyRole)
        { 
            Role role = await _context.Role
                .Where(role => role.Uid == role.SiteUid && role.Rno == role.SiteRno)
                .Include(role => role.Utilisateur).ThenInclude(u => u.ApplicationUser)
                .Include(role => role.Site)
                .Include(role => role.Archives)
                .FirstOrDefaultAsync();
            Fournisseur fournisseur = new Fournisseur(role);
            return fournisseur;
        }

    }
}
