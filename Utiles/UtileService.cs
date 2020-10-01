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
        /// retourne la date du catalogue du site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<DateTime> DateCatalogue(AKeyUidRno keySite)
        {
            ArchiveProduit archiveProduit = await _context.ArchiveProduit
                .Where(a => keySite.Uid == a.Uid && keySite.Rno == a.Rno)
                .OrderBy(a => a.Date)
                .LastAsync();
            DateTime dateProduits = archiveProduit.Date;
            ArchiveCatégorie archiveCatégorie = await _context.ArchiveCatégorie
                .Where(a => keySite.Uid == a.Uid && keySite.Rno == a.Rno)
                .OrderBy(a => a.Date)
                .LastAsync();
            DateTime dateCatégories = archiveCatégorie.Date;
            return dateCatégories < dateProduits ? dateProduits : dateCatégories;
        }

        /// <summary>
        /// Retourne une fonction qui filtre les roles qui appartiennent à un site
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        public Func<Role, bool> FiltreSite(AKeyUidRno keySite)
        {
            bool filtreSite(Role role) => role.SiteUid == keySite.Uid && role.SiteRno == keySite.Rno;
            return filtreSite;
        }

        /// <summary>
        /// retourne un filtre que ne passent que les roles d'Etat Actif ou Nouveau
        /// </summary>
        /// <returns></returns>
        public Func<Role, bool> FiltreRoleActif()
        {
            static bool filtreRole(Role role) => role.Etat == TypeEtatRole.Actif || role.Etat == TypeEtatRole.Nouveau;
            return filtreRole;
        }

        /// <summary>
        /// Retourne un IQueryable qui renvoie les Clients passant les filtres présents et qui inclut les champs Role et Role.Site
        /// </summary>
        /// <param name="keyClient">si présent, unseul client est retourné</param>
        /// <returns></returns>
        public IQueryable<Client> ClientsAvecRoleEtSite(AKeyUidRno keyClient)
        {
            IQueryable<Client> query = _context.Client;
            if (keyClient != null)
            {
                query = query.Where(cl => cl.Uid == keyClient.Uid && cl.Rno == keyClient.Rno);
            }
            query = query.Include(c => c.Role).ThenInclude(r => r.Site);
            return query;
        }

        public async Task<Produit> Produit(Site site, long No)
        {
            return await _context.Produit
                .Where(p => p.Uid == site.Uid && p.Rno == site.Rno && p.No == No)
                .Include(p => p.ArchiveProduits)
                .FirstOrDefaultAsync();
        }

    }
}
