using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
        public GèreArchive(DbSet<Site> dbSet, DbSet<ArchiveSite> dbSetArchive) : base(dbSet, dbSetArchive)
        { }

        protected override ArchiveSite CréeArchive()
        {
            return new ArchiveSite();
        }

        protected override void CopieDonnéeDansArchive(Site donnée, ArchiveSite archive)
        {
            archive.Url = donnée.Url;
            archive.Titre = donnée.Titre;
            archive.Nom = donnée.Nom;
            archive.Adresse = donnée.Adresse;
            archive.Ville = donnée.Ville;
            archive.Etat = donnée.Etat;
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
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                donnée.Nom = vue.Nom;
                archive.Nom = vue.Nom;
                modifié = true;
            }
            if (vue.Adresse != null && donnée.Adresse != vue.Adresse)
            {
                donnée.Adresse = vue.Adresse;
                archive.Adresse = vue.Adresse;
                modifié = true;
            }
            if (vue.Ville != null && donnée.Ville != vue.Ville)
            {
                donnée.Ville = vue.Ville;
                archive.Ville = vue.Ville;
                modifié = true;
            }
            if (vue.Etat != null && donnée.Etat != vue.Etat)
            {
                donnée.Etat = vue.Etat;
                archive.Etat = vue.Etat;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class SiteService : KeyUidRnoService<Site, SiteVue>, ISiteService
    {
        private readonly IUtileService _utile;
        private readonly IClientService _clientService;

        public SiteService(ApplicationContext context,
            IUtileService utile,
            IClientService clientService
            ) : base(context)
        {
            _dbSet = _context.Site;
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveSite);
            _utile = utile;
            _clientService = clientService;
            dCréeVueAsync = CréeSiteVueAsync;
            dValideEdite = ValideEdite;
        }

        private Task<SiteVue> CréeSiteVueAsync(Site site)
        {
            SiteVue vue = new SiteVue
            {
                Uid = site.Uid,
                Rno = site.Rno,
                Url = site.Url,
                Titre = site.Titre,
                Nom = site.Nom,
                Adresse = site.Adresse,
                Ville = site.Ville,
                FormatNomFichierCommande = site.FormatNomFichierCommande,
                FormatNomFichierLivraison = site.FormatNomFichierLivraison,
                FormatNomFichierFacture = site.FormatNomFichierFacture,
                Etat = site.Etat,
            };
            return Task.FromResult(vue);
        }

        public async Task FixeNbs(List<SiteVue> siteVues)
        {
            foreach (SiteVue siteVue in siteVues)
            {
                await FixeNbs(siteVue);
            }
        }

        private async Task FixeNbs(SiteVue siteVue)
        {
            siteVue.NbProduits = await _utile.NbDisponibles(siteVue);
            siteVue.NbClients = await _clientService.NbClients(siteVue);
        }

        public async Task<SiteVue> LitNbs(Site site)
        {
            SiteVue siteVue = new SiteVue();
            siteVue.Copie(site);
            await FixeNbs(siteVue);
            return siteVue;
        }

        public async Task<SiteVue> TrouveVueParUrl(string url)
        {
            Site site = await _context.Site.Where(s => s.Url == url).FirstOrDefaultAsync();
            return site == null ? null : CréeVue(site);
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

        private static SiteVue CréeVueAvecNbs(Site site)
        {
            int nbProduits = 0;
            foreach (Catégorie catégorie in site.Catégories)
            {
                nbProduits += catégorie.Produits
                    .Where(p => p.Etat == TypeEtatProduit.Disponible)
                    .Count();
            }
            SiteVue vue = new SiteVue
            {
                NbClients = site.Usagers
                    .Where(u => u.Uid != site.Uid) // pas fournisseur
                    .Where(u => u.Etat == TypeEtatRole.Actif || u.Etat == TypeEtatRole.Nouveau)
                    .Count(),
                NbProduits = nbProduits
            };
            vue.Copie(site);
            return vue;
        }

        protected override async Task<List<SiteVue>> CréeVues(KeyParam param)
        {
            IQueryable<Site> sites = DbSetFiltré(param)
                .Include(s => s.Catégories)
                .ThenInclude(c => c.Produits)
                .Include(s => s.Usagers);
            var res1 = await sites.ToListAsync();

            IQueryable<SiteVue> vues = sites
                .Select(site => CréeVueAvecNbs(site));

            List<SiteVue> liste = await vues.ToListAsync();
            return liste;
        }


        // implémentation des membres abstraits
        public override void CopieVueDansDonnée(Site donnée, SiteVue vue)
        {
            if (vue.Url != null)
            {
                donnée.Url = vue.Url;
            }
            if (vue.Titre != null)
            {
                donnée.Titre = vue.Titre;
            }
            if (vue.Nom != null)
            {
                donnée.Nom = vue.Nom;
            }
            if (vue.Adresse != null)
            {
                donnée.Adresse = vue.Adresse;
            }
            if (vue.Ville != null)
            {
                donnée.Ville = vue.Ville;
            }
            donnée.FormatNomFichierCommande = vue.FormatNomFichierCommande;
            donnée.FormatNomFichierLivraison = vue.FormatNomFichierLivraison;
            donnée.FormatNomFichierFacture = vue.FormatNomFichierFacture;
        }

        public override void CopieVuePartielleDansDonnée(Site donnée, SiteVue vue, Site donnéePourComplèter)
        {
            donnée.Url = vue.Url ?? donnéePourComplèter.Url;
            donnée.Titre = vue.Titre ?? donnéePourComplèter.Titre;
            donnée.Etat = vue.Etat ?? donnéePourComplèter.Etat;
            donnée.Nom = vue.Nom ?? donnéePourComplèter.Nom;
            donnée.Adresse = vue.Adresse ?? donnéePourComplèter.Adresse;
            donnée.Ville = vue.Ville ?? donnéePourComplèter.Ville;
            donnée.FormatNomFichierCommande = vue.FormatNomFichierCommande ?? donnéePourComplèter.FormatNomFichierCommande;
            donnée.FormatNomFichierLivraison = vue.FormatNomFichierLivraison ?? donnéePourComplèter.FormatNomFichierLivraison;
            donnée.FormatNomFichierFacture = vue.FormatNomFichierFacture ?? donnéePourComplèter.FormatNomFichierFacture;
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
                Nom = donnée.Nom,
                Adresse = donnée.Adresse,
                Ville = donnée.Ville,
                FormatNomFichierCommande = donnée.FormatNomFichierCommande,
                FormatNomFichierLivraison = donnée.FormatNomFichierLivraison,
                FormatNomFichierFacture = donnée.FormatNomFichierFacture,
                Etat = donnée.Etat,
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }

        public override async Task<SiteVue> LitVue(KeyParam param)
        {
            Site site = await _context.Site.Where(e => e.Uid == param.Uid && e.Rno == param.Rno).FirstOrDefaultAsync();
            SiteVue vue = new SiteVue
            {
                Url = site.Url,
                Titre = site.Titre,
                Nom = site.Nom,
                Adresse = site.Adresse,
                Ville = site.Ville,
                FormatNomFichierCommande = site.FormatNomFichierCommande,
                FormatNomFichierLivraison = site.FormatNomFichierLivraison,
                FormatNomFichierFacture = site.FormatNomFichierFacture,
                Etat = site.Etat,
            };
            site.CopieKey(vue.KeyParam);
            return vue;
        }

        /// <summary>
        /// Retourne une vue ne contenant que l'état ou null
        /// </summary>
        /// <param name="akeySite"></param>
        /// <returns></returns>
        public async Task<SiteVue> Etat(AKeyUidRno akeySite)
        {
            Site site = await _context.Site.Where(e => e.Uid == akeySite.Uid && e.Rno == akeySite.Rno).FirstAsync();
            if (site == null)
            {
                return null;
            }
            SiteVue vue = new SiteVue
            {
                Etat = site.Etat,
            };
            site.CopieKey(vue.KeyParam);
            return vue;

        }

        /// <summary>
        /// vérifie que le changement de l'état du site pour celui de la vue est possible
        /// </summary>
        /// <param name="site"></param>
        /// <param name="vue"></param>
        /// <param name="modelState"></param>
        /// <param name="estAdministrateur"></param>
        /// <returns></returns>
        public async Task ValideChangeEtat(Site site, SiteVue vue, ModelStateDictionary modelState, bool estAdministrateur)
        {
            if (!TypeEtatSite.EstValide(vue.Etat))
            {
                ErreurDeModel.AjouteAModelState(modelState, "livraisonEnCours");
                return;
            }

            if (site.Etat == vue.Etat)
            {
                ErreurDeModel.AjouteAModelState(modelState, "etatIncorrect");
                return;
            }

            /// I: interdit F: fournisseur A: administrateur ?: avec conditions
            /// vue ->      Ouvert  Catalogue   Livraison   Banni
            /// Ouvert          I       F           F         A
            /// Catalogue       F       I           I         A
            /// Livraison       F       I           I         A
            /// Banni           A       I           I         I

            // seul un administrateur peut utiliser l'état Banni
            if (!estAdministrateur && (site.Etat == TypeEtatSite.Banni || vue.Etat == TypeEtatSite.Banni))
            {
                ErreurDeModel.AjouteAModelState(modelState, "etatBanni");
                return;
            }

            // seul l'état Ouvert ou Banni peut suivre un état qui n'est pas Ouvert
            if (site.Etat != TypeEtatSite.Ouvert && vue.Etat != TypeEtatSite.Ouvert && vue.Etat != TypeEtatSite.Banni)
            {
                ErreurDeModel.AjouteAModelState(modelState, "etatIncorrect");
                return;
            }

            // impossible de quitter l'état Catalogue si le site n'a pas de produits
            // comme le site est créé dans l'état Catalogue, il ne peut pas être sans produits sans être dans l'état Catalogue
            if (site.Etat == TypeEtatSite.Catalogue && vue.Etat != TypeEtatSite.Catalogue)
            {
                int produits = await _utile.NbDisponibles(site);
                if (produits == 0)
                {
                    ErreurDeModel.AjouteAModelState(modelState, "catalogueVide");
                    return;
                }
            }

            // si le site n'a pas de produits, il doit avoir l'état Catalogue quand il quitte l'état Banni
            if (site.Etat == TypeEtatSite.Banni)
            {
                int produits = await _utile.NbDisponibles(site);
                if (produits == 0)
                {
                    vue.Etat = TypeEtatSite.Catalogue;
                    return;
                }
                
            }

        }

        /// <summary>
        /// change l'état du site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        public async Task<RetourDeService> ChangeEtat(Site site, string nouvelEtat)
        {
            DateTime maintenant = DateTime.Now;
            site.Etat = nouvelEtat;
            _context.Site.Update(site);
            // ajout de l'archive
            ArchiveSite archive = new ArchiveSite
            {
                Uid = site.Uid,
                Rno = site.Rno,
                Etat = nouvelEtat,
                Date = maintenant
            };
            _context.ArchiveSite.Add(archive);

            return await SaveChangesAsync();
        }

        public async Task<bool> UrlPrise(string Url)
        {
            return await _dbSet.Where(site => site.Url == Url).AnyAsync();
        }

        public async Task<bool> UrlPriseParAutre(AKeyUidRno key, string Url)
        {
            return await _dbSet.Where(site => site.Url == Url && (site.Uid != key.Uid || site.Rno != key.Rno)).AnyAsync();
        }

        public async Task<bool> TitrePris(string titre)
        {
            return await _dbSet.Where(site => site.Titre == titre).AnyAsync();
        }

        public async Task<bool> TitrePrisParAutre(AKeyUidRno key, string titre)
        {
            return await _dbSet.Where(site => site.Titre == titre && (site.Uid != key.Uid || site.Rno != key.Rno)).AnyAsync();
        }

        private async Task ValideEdite(Site donnée, ModelStateDictionary modelState)
        {
            if (await UrlPriseParAutre(donnée, donnée.Url))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris", "Url");
            }
            if (await TitrePrisParAutre(donnée, donnée.Titre))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris", "titre");
            }
        }
    }
}
