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

        public async Task<List<FournisseurVue>> Fournisseurs()
        {
            List<Fournisseur> fournisseurs = await _context.Fournisseur
                .Where(fournisseur => fournisseur.UtilisateurId != null)
                .Include(fournisseur => fournisseur.Utilisateur)
                .Include(fournisseur => fournisseur.Site)
                .Include(fournisseur => fournisseur.Archives)
                .AsNoTracking()
                .ToListAsync();
            List<FournisseurVue> vues = fournisseurs.Select(fournisseur => new FournisseurVue(fournisseur)).ToList();
            return vues;
        }

        public async Task<List<DemandeFournisseurVue>> DemandesFournisseurs()
        {
            List<DemandeSite> fournisseurs = await _context.DemandeSite
                .Include(demande => demande.Fournisseur).ThenInclude(fournisseur => fournisseur.Site)
                .AsNoTracking()
                .ToListAsync();
            List<DemandeFournisseurVue> vues = fournisseurs.Select(demande => new DemandeFournisseurVue(demande)).ToList();
            return vues;
        }

        public async Task<FournisseurVue> Fournisseur(uint idFournisseur)
        { 
            Fournisseur fournisseur = await _context.Fournisseur
                .Where(f => f.Id == idFournisseur)
                .Include(f => f.Utilisateur).ThenInclude(u => u.Archives)
                .Include(f => f.Site)
                .Include(f => f.Archives)
                .FirstOrDefaultAsync();
            if (fournisseur == null)
            {
                return null;
            }
            return new FournisseurVue(fournisseur);
        }

    }
}
