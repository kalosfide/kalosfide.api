using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Roles;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{
    class NbDeSite
    {
        public string Uid { get; set; }
        public int Rno { get; set; }
        public int Nb { get; set; }
    }

    class GèreArchive : GéreArchiveUidRno<Site, SiteVue, ArchiveSite>
    {
        public GèreArchive(
            DbSet<Site> dbSet,
            IIncludableQueryable<Site, ICollection<ArchiveSite>> query,
            Func<Site, ICollection<ArchiveSite>> archives,
            DbSet<ArchiveSite> dbSetArchive,
            IIncludableQueryable<ArchiveSite, Site> queryArchive,
            Func<ArchiveSite, Site> donnée
            ) : base(dbSet, query, archives, dbSetArchive, queryArchive, donnée)
        { }

        protected override ArchiveSite CréeArchive()
        {
            return new ArchiveSite();
        }

        protected override void CopieDonnéeDansArchive(Site donnée, ArchiveSite archive)
        {
            archive.Url = donnée.Url;
            archive.Titre = donnée.Titre;
            archive.Ouvert = donnée.Ouvert;
        }

        protected override ArchiveSite CréeArchiveDesDifférences(Site donnée, SiteVue vue)
        {
            bool modifié = false;
            ArchiveSite archive = new ArchiveSite();
            if (vue.Url != null && donnée.Url != vue.Url)
            {
                donnée.Url = vue.Url;
                archive.Url = vue.Url;
                modifié = true;
            }
            if (vue.Titre != null && donnée.Titre != vue.Titre)
            {
                donnée.Titre = vue.Titre;
                archive.Titre = vue.Titre;
                modifié = true;
            }
            if (vue.Ouvert != null && donnée.Ouvert != vue.Ouvert)
            {
                donnée.Ouvert = vue.Ouvert.Value;
                archive.Ouvert = vue.Ouvert;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class SiteService : KeyUidRnoService<Site, SiteVue>, ISiteService
    {
        private readonly IUtileService _utile;
        private readonly IRoleService _roleService;
        private readonly IClientService _clientService;

        public SiteService(ApplicationContext context,
            IUtileService utile,
            IRoleService roleService,
            IClientService clientService
            ) : base(context)
        {
            _dbSet = _context.Site;
            _géreArchive = new GèreArchive(
                _dbSet, _dbSet.Include(site => site.Archives), (Site site) => site.Archives,
                _context.ArchiveSite, _context.ArchiveSite.Include(a => a.Site), (ArchiveSite archive) => archive.Site
                );
            _utile = utile;
            _roleService = roleService;
            _clientService = clientService;
            dCréeVueAsync = CréeSiteVueAsync;
            dValideEdite = ValideEdite;
        }

        public async Task<RetourDeService<Role>> CréeRoleSite(Utilisateur utilisateur, ICréeSiteVue vue)
        {
            string uid = utilisateur.Uid;
            int rno = (await _roleService.DernierNo(uid)) + 1;

            Role role = new Role
            {
                Uid = uid,
                Rno = rno,
                SiteUid = uid,
                SiteRno = rno,
                Etat = TypeEtatRole.Actif,
                Nom = vue.Nom,
                Adresse = vue.Adresse,
                Ville = vue.Ville
            };
            _roleService.AjouteSansSauver(role);

            DateTime maintenant = DateTime.Now;
            Site site = new Site
            {
                Uid = uid,
                Rno = rno,
                Url = vue.Url,
                Titre = vue.Titre,
                Ouvert = false,
                // null tant que la première modification du catalogue (qui crée le catalogue) n'est pas terminée.
                DateCatalogue = null
            };
            AjouteSansSauver(site, maintenant);
            role.Site = site;

            return await SaveChangesAsync(role);
        }

        private Task<SiteVue> CréeSiteVueAsync(Site site)
        {
            SiteVue vue = new SiteVue
            {
                Uid = site.Uid,
                Rno = site.Rno,
                Url = site.Url,
                Titre = site.Titre,
                Ouvert = site.Ouvert,
            };
            return Task.FromResult(vue);
        }

        public async Task<Site> TrouveParUrl(string url)
        {
            Site site = await _context.Site.Where(s => s.Url == url).FirstOrDefaultAsync();
            return site;
        }

        public async Task<Site> TrouveParKey(string Uid, int Rno)
        {
            Site site = await _context.Site.Where(s => s.Uid == Uid && s.Rno == Rno).FirstOrDefaultAsync();
            return site;
        }

        // implémentation des membres abstraits
        protected override void CopieVueDansDonnée(SiteVue de, Site vers)
        {
            if (de.Url != null)
            {
                vers.Url = de.Url;
            }
            if (de.Titre != null)
            {
                vers.Titre = de.Titre;
            }
        }

        protected override void CopieVuePartielleDansDonnée(SiteVue de, Site vers, Site pourComplèter)
        {
            vers.Url = de.Url ?? pourComplèter.Url;
            vers.Titre = de.Titre ?? pourComplèter.Titre;
            vers.Ouvert = de.Ouvert ?? pourComplèter.Ouvert;
        }

        public override Site CréeDonnée()
        {
            return new Site();
        }

        public override SiteVue CréeVue(Site donnée)
        {
            SiteVue vue = new SiteVue
            {
                Url = donnée.Url,
                Titre = donnée.Titre,
                Ouvert = donnée.Ouvert,
            };
            vue.CopieKey(donnée);
            return vue;
        }

        public async Task<List<SiteVue>> ListeVues()
        {
            List<Site> sites = await _context.Site.ToListAsync();
            return sites
                .Select(s => CréeVue(s))
                .ToList();
        }

        /// <summary>
        /// Enregistre le commencement de la modification du catalogue du site.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<RetourDeService<ArchiveSite>> CommenceEtatCatalogue(Site site)
        {
            site.Ouvert = false;
            _context.Site.Update(site);
            // ajout de l'archive
            ArchiveSite archive = new ArchiveSite
            {
                Uid = site.Uid,
                Rno = site.Rno,
                Ouvert = false,
                Date = DateTime.Now
            };
            _context.ArchiveSite.Add(archive);

            return await SaveChangesAsync(archive);
        }

        /// <summary>
        /// Enregistre la fin de la modification du catalogue du site.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="dateCatalogue">présent si le catalogue a été modifié</param>
        /// <returns></returns>
        public async Task<RetourDeService<ArchiveSite>> TermineEtatCatalogue(Site site, DateTime? dateCatalogue)
        {
            site.Ouvert = true;
            if (dateCatalogue.HasValue)
            {
                site.DateCatalogue = dateCatalogue.Value;
            }
            _context.Site.Update(site);
            // ajout de l'archive
            ArchiveSite archive = new ArchiveSite
            {
                Uid = site.Uid,
                Rno = site.Rno,
                Ouvert = true,
                Date = dateCatalogue ?? DateTime.Now
            };
            _context.ArchiveSite.Add(archive);

            return await SaveChangesAsync(archive);
        }

        /// <summary>
        /// Vérifie s'il existe un Site ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        public async Task<bool> UrlPrise(string url)
        {
            return await _dbSet.Where(site => site.Url == url).AnyAsync();
        }

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par la KeyUidRno ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <param name="key">Objet ayant la KeyUidRno d'un Site</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        public async Task<bool> UrlPriseParAutre(AKeyUidRno key, string url)
        {
            return await _dbSet.Where(site => site.Url == url && (site.Uid != key.Uid || site.Rno != key.Rno)).AnyAsync();
        }

        /// <summary>
        /// Vérifie s'il existe un Site ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        public async Task<bool> TitrePris(string titre)
        {
            return await _dbSet.Where(site => site.Titre == titre).AnyAsync();
        }

        /// <summary>
        /// Vérifie s'il existe un Site autre que celui défini par la KeyUidRno ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <param name="key">Objet ayant la KeyUidRno d'un Site</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        public async Task<bool> TitrePrisParAutre(AKeyUidRno key, string titre)
        {
            return await _dbSet.Where(site => site.Titre == titre && (site.Uid != key.Uid || site.Rno != key.Rno)).AnyAsync();
        }

        private async Task ValideEdite(Site donnée, ModelStateDictionary modelState)
        {
            if (await UrlPriseParAutre(donnée, donnée.Url))
            {
                ErreurDeModel.AjouteAModelState(modelState, "Url", "nomPris");
            }
            if (await TitrePrisParAutre(donnée, donnée.Titre))
            {
                ErreurDeModel.AjouteAModelState(modelState, "titre", "nomPris");
            }
        }
    }
}
