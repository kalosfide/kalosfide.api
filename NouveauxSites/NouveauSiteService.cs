using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.NouveauxSites
{
    public class NouveauSiteService: BaseService, INouveauSiteService
    {
        private readonly ISiteService _siteService;
        private readonly IRoleService _roleService;

        public NouveauSiteService(ApplicationContext context,
            ISiteService siteService,
            IRoleService roleService
            ) : base(context)
        {
            _siteService = siteService;
            _roleService = roleService;
        }

        /// <summary>
        /// Cherche un NouveauSite à partir de son Email
        /// </summary>
        /// <param name="email">Email du NouveauSite recherché</param>
        /// <returns>le NouveauSite trouvé, ou null.</returns>
        public async Task<NouveauSite> NouveauSite(string email)
        {
            return await _context.NouveauxSites.Where(ns => ns.Email == email).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Vérifie s'il existe un NouveauSite ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du NouveauSite recherché</param>
        /// <returns>true s'il existe un NouveauSite ayant cette Url</returns>
        public async Task<bool> UrlPrise(string url)
        {
            return await _context.NouveauxSites.Where(ns => ns.Url == url).AnyAsync();
        }

        /// <summary>
        /// Vérifie s'il existe un NouveauSite ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du NouveauSite recherché</param>
        /// <returns>true s'il existe un NouveauSite ayant ce Titre</returns>
        public async Task<bool> TitrePris(string titre)
        {
            return await _context.NouveauxSites.Where(ns => ns.Titre == titre).AnyAsync();
        }

        /// <summary>
        /// Enregistre dans la bdd un NouveauSite créé à partir de la vue.
        /// </summary>
        /// <param name="vue"></param>
        /// <returns></returns>
        public async Task<RetourDeService> EnregistreDemande(NouveauSiteDemande vue)
        {
            DateTime maintenant = DateTime.Now;
            NouveauSite nouveauSite = new NouveauSite { Email = vue.Email, Date = maintenant };
            Role.CopieDef(vue, nouveauSite);
            Site.CopieDef(vue, nouveauSite);
            _context.NouveauxSites.Add(nouveauSite);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Ajoute à la bdd un Role et un Site créés à partir d'un NouveauSite.
        /// Si la création réussit, supprime le NouveauSite de la bdd et ajpute le Role aux Roles de l'Utilisateur.
        /// </summary>
        /// <param name="utilisateur">Utilisateur propriétaire du Role et du Site à créer</param>
        /// <param name="nouveauSite">NouveauSite contenant les données du Role et du Site à créer</param>
        /// <returns>un RetourDeService contenant le Role créé incluant son Site ou une erreur</returns>
        public async Task<RetourDeService<Role>> CréeRoleEtSite(Utilisateur utilisateur, NouveauSite nouveauSite)
        {
            int rno = utilisateur.Roles.Count + 1;
            Role role = new Role
            {
                Uid = utilisateur.Uid,
                Rno = rno,
                SiteUid = utilisateur.Uid,
                SiteRno = rno
            };
            Role.CopieDef(nouveauSite, role);
            RetourDeService<Role> retourRole = await _roleService.Ajoute(role);
            if (!retourRole.Ok)
            {
                return retourRole;
            }
            Site site = new Site
            {
                Uid = utilisateur.Uid,
                Rno = rno
            };
            Site.CopieDef(nouveauSite, site);
            RetourDeService<Site> retourSite = await _siteService.Ajoute(site);
            if (!retourSite.Ok)
            {
                return new RetourDeService<Role>(retourSite);
            }
            retourRole.Entité.Site = site;
            utilisateur.Roles.Add(retourRole.Entité);
            return retourRole;
        }

    }
}
