using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public class UtileService : IUtileService
    {
        private readonly ApplicationContext _context;
        private readonly IRoleService _roleService;

        public UtileService(ApplicationContext context,
            IRoleService roleService
            )
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task<Site> SiteDeKey(AKeyUidRno akeySite)
        {
            return await _context.Site.Where(site => site.Uid == akeySite.Uid && site.Rno == akeySite.Rno).FirstOrDefaultAsync();
        }

        /// <summary>
        /// retourne le Site du Role défini par keyClient si le Role n'est pas celui du Fournisseur du Site
        /// </summary>
        /// <param name="keyClient"></param>
        /// <returns>null si la clé n'est pas celle d'un client d'un site</returns>
        public async Task<Site> SiteDeClient(AKeyUidRno keyClient)
        {
            Site site = await _roleService.SiteDeRole(keyClient);
            /* le Role du Fournisseur du Site a le même Uid que le Site */
            return site != null && site.Uid != keyClient.Uid ? site : null;
        }

        /// <summary>
        /// retourne le site du produit ou de la livraison
        /// </summary>
        /// <param name="keyProduitOuLivraison"></param>
        /// <returns></returns>
        public async Task<Site> SiteDeKeyProduitOuLivraison(KeyUidRnoNo keyProduitOuLivraison)
        {
            Site site = await _context.Site.Where(s => s.Uid == keyProduitOuLivraison.Uid && s.Rno == keyProduitOuLivraison.Rno).FirstOrDefaultAsync();
            return site;
        }

        /// <summary>
        /// retourne le nombre de produits disponibles du site
        /// </summary>
        /// <param name="keySite">Site ou SiteVue ou keyUidRno</param>
        /// <returns></returns>
        public async Task<int> NbDisponibles(AKeyUidRno keySite)
        {
            return await _context.Produit
                .Where(p => p.Uid == keySite.Uid && p.Rno == keySite.Rno && p.Etat == TypeEtatProduit.Disponible)
                .CountAsync();
        }

        /// <summary>
        /// Retourne le Role (qui inclut le champ Site) correspondant à la key s'il sagit de celui d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns></returns>
        public async Task<Role> ClientRoleAvecSite(AKeyUidRno keyClient)
        {
            Role role = await _context.Role
                .Where(cl => cl.Uid == keyClient.Uid && cl.Rno == keyClient.Rno)
                .Where(cl => cl.Uid != cl.SiteUid || cl.Rno != cl.SiteRno)
                .FirstOrDefaultAsync();
            return role;
        }

        public async Task<Produit> Produit(Site site, long No)
        {
            return await _context.Produit
                .Where(p => p.Uid == site.Uid && p.Rno == site.Rno && p.No == No)
                .Include(p => p.Archives)
                .FirstOrDefaultAsync();
        }

    }
}
