using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{

    public class GèreArchiveSite : AvecIdUintGèreArchive<Site, SiteAEditer, ArchiveSite>
    {
        public GèreArchiveSite(DbSet<ArchiveSite> dbSetArchive) : base(dbSetArchive)
        { }

        protected override ArchiveSite CréeArchive()
        {
            return new ArchiveSite();
        }

        protected override void CopieDonnéeDansArchive(Site donnée, ArchiveSite archive)
        {
            Site.CopieData(donnée, archive);
            archive.Ouvert = donnée.Ouvert;
        }

        protected override ArchiveSite CréeArchiveDesDifférences(Site donnée, SiteAEditer vue)
        {
            ArchiveSite archive = new ArchiveSite
            {
                Date = DateTime.Now
            };
            bool modifié = Site.CopieDifférences(donnée, vue, archive);
            return modifié ? archive : null;
        }
    }

    public class SiteService : AvecIdUintService<Site, ISiteData, Site, SiteAEditer>, ISiteService
    {
        private readonly IUtileService _utile;

        public SiteService(ApplicationContext context,
            IUtileService utile
            ) : base(context)
        {
            _dbSet = _context.Site;
            _gèreArchive = new GèreArchiveSite(_context.ArchiveSite);
            _utile = utile;
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
        }

        public GèreArchiveSite GèreArchiveSite { get { return (GèreArchiveSite)_gèreArchive; } }

        public async Task<Site> TrouveParUrl(string url)
        {
            Site site = await _context.Site.Where(s => s.Url == url).FirstOrDefaultAsync();
            return site;
        }

        public async Task<bool> TrouveParId(uint id)
        {
            return await _context.Site.Where(s => s.Id == id).AnyAsync();
        }

        // implémentation des membres abstraits
        public override Site CréeDonnée()
        {
            return new Site();
        }

        protected override Site Ajouté(Site donnée, DateTime date)
        {
            Site ajouté = new Site
            {
                Id = donnée.Id,
            };
            Site.CopieData(donnée, ajouté);
            return ajouté;
        }

        protected override void CopieAjoutDansDonnée(ISiteData de, Site vers)
        {
            Site.CopieData(de, vers);
        }

        protected override void CopieEditeDansDonnée(SiteAEditer de, Site vers)
        {
            Site.CopieDataSiPasNull(de, vers);
        }

        protected override void CopieVuePartielleDansDonnée(SiteAEditer de, Site vers, Site pourCompléter)
        {
            Site.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
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
                Id = site.Id,
                Ouvert = false,
                Date = DateTime.Now
            };
            _context.ArchiveSite.Add(archive);

            return await SaveChangesAsync(archive);
        }

        /// <summary>
        /// Enregistre l'ouverture du site et archive avec éventuellement la date de la fin de la modification du catalogue.
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
                Id = site.Id,
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
        /// Vérifie s'il existe un Site autre que celui défini par l'Id ayant une certaine Url.
        /// </summary>
        /// <param name="url">Url du Site à rechercher</param>
        /// <param name="id">Id d'un Site</param>
        /// <returns>true s'il existe un Site ayant cette Url</returns>
        public async Task<bool> UrlPriseParAutre(uint id, string url)
        {
            return await _dbSet.Where(site => site.Url == url && site.Id != id).AnyAsync();
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
        /// Vérifie s'il existe un Site autre que celui défini par l'Id ayant un certain Titre.
        /// </summary>
        /// <param name="titre">Titre du Site à rechercher</param>
        /// <param name="id">Id d'un Site</param>
        /// <returns>true s'il existe un Site ayant ce Titre</returns>
        public async Task<bool> TitrePrisParAutre(uint id, string titre)
        {
            return await _dbSet.Where(site => site.Titre == titre && site.Id != id).AnyAsync();
        }

        private async Task ValideAjoute(Site site, ModelStateDictionary modelState)
        {
            if (ClientApp.NomRéservé(site.Url) || await UrlPrise(site.Url))
            {
                ErreurDeModel.AjouteAModelState(modelState, "site.url", "nomPris");
            }
            if (ClientApp.NomRéservé(site.Url) || await TitrePris(site.Titre))
            {
                ErreurDeModel.AjouteAModelState(modelState, "site.titre", "nomPris");
            }
        }

        private async Task ValideEdite(Site site, ModelStateDictionary modelState)
        {
            if (ClientApp.NomRéservé(site.Url) || await UrlPriseParAutre(site.Id, site.Url))
            {
                ErreurDeModel.AjouteAModelState(modelState, "url", "nomPris");
            }
            if (ClientApp.NomRéservé(site.Url) || await TitrePrisParAutre(site.Id, site.Titre))
            {
                ErreurDeModel.AjouteAModelState(modelState, "titre", "nomPris");
            }
        }

        /// <summary>
        /// Indique s'il y a ou s'il y a eu des Invitations pour un Site.
        /// </summary>
        /// <param name="id">Id du Site</param>
        /// <returns>retourne le nombre d'invitations en cours ou auxquelles un client a répondu.</returns>
        public async Task<int> AvecInvitations(uint id)
        {
            int aInvitations = await _context.Invitation.Where(i => i.Id == id).CountAsync();
            aInvitations += await _context.Client.Where(c => c.SiteId == id && c.UtilisateurId != null).CountAsync();
            return aInvitations;
        }
    }
}
