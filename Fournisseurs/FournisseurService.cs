using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    class GèreArchive: AvecIdUintGèreArchive<Fournisseur, FournisseurAEditer, ArchiveFournisseur>
    {
        private readonly GèreArchiveSite _gèreArchiveSite;
        public GèreArchive(DbSet<ArchiveFournisseur> dbSetArchive, GèreArchiveSite gèreArchiveSite) : base(dbSetArchive)
        {
            _gèreArchiveSite = gèreArchiveSite;
        }

        protected override ArchiveFournisseur CréeArchive()
        {
            return new ArchiveFournisseur();
        }

        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date en paramétre.
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="date"></param>
        public override void GèreAjout(Fournisseur donnée, DateTime date)
        {
            base.GèreAjout(donnée, date);
            _gèreArchiveSite.GèreAjout(donnée.Site, date);
        }

        protected override void CopieDonnéeDansArchive(Fournisseur donnée, ArchiveFournisseur archive)
        {
            Fournisseur.CopieData(donnée, archive);
        }

        protected override ArchiveFournisseur CréeArchiveDesDifférences(Fournisseur donnée, FournisseurAEditer vue)
        {
            ArchiveFournisseur archive = new ArchiveFournisseur
            {
                Id = donnée.Id,
                Date = DateTime.Now
            };
            bool modifié = Fournisseur.CopieDifférences(donnée, vue, archive);
            return modifié ? archive : null;
        }
    }
    public class FournisseurService : AvecIdUintService<Fournisseur, FournisseurAAjouter, Fournisseur, FournisseurAEditer>, IFournisseurService
    {
        private readonly ISiteService _siteService;
        private readonly IEnvoieEmailService _emailService;


        public FournisseurService(
            ApplicationContext context,
            IEnvoieEmailService emailService,
            ISiteService siteService
        ) : base(context)
        {
            _emailService = emailService;
            _siteService = siteService;
            _dbSet = _context.Fournisseur;
            _gèreArchive = new GèreArchive(_context.ArchiveFournisseur, _siteService.GèreArchiveSite);
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
        }

        public async Task<bool> NomPris(string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(uint id, string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom && c.Id != id).AnyAsync();
        }

        private async Task ValideAjoute(Fournisseur donnée, ModelStateDictionary modelState)
        {
            if (await NomPris(donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
            await _siteService.DValideAjoute()(donnée.Site, modelState);
        }

        private async Task ValideEdite(Fournisseur donnée, ModelStateDictionary modelState)
        {
            if (await NomPrisParAutre(donnée.Id, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
        }

        public override Fournisseur CréeDonnée()
        {
            return new Fournisseur();
        }

        protected override Fournisseur Ajouté(Fournisseur donnée, DateTime date)
        {
            Fournisseur ajouté = new Fournisseur
            {
                Id = donnée.Id,
            };
            Fournisseur.CopieData(donnée, ajouté);
            ajouté.Site = new Site
            {
                Id = donnée.Id
            };
            Site.CopieData(donnée.Site, ajouté.Site);
            return ajouté;
        }

        protected override void CopieAjoutDansDonnée(FournisseurAAjouter de, Fournisseur vers)
        {
            Fournisseur.CopieData(de, vers);
            vers.Etat = EtatRole.Nouveau;
            vers.Site = new Site
            {
                Id = vers.Id
            };
            Site.CopieData(de.Site, vers.Site);
            vers.Site.Ouvert = false;
        }

        protected override void CopieEditeDansDonnée(FournisseurAEditer de, Fournisseur vers)
        {
            Fournisseur.CopieDataSiPasNull(de, vers);    
        }

        protected override void CopieVuePartielleDansDonnée(FournisseurAEditer de, Fournisseur vers, Fournisseur pourCompléter)
        {
            Fournisseur.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
        }

        /// <summary>
        /// Cherche une demande de création de site à partir de l'Email.
        /// </summary>
        /// <param name="email">Email de la DemandeSite cherchée</param>
        /// <returns>si elle existe, la DemandeSite retournée n'inclut pas son Fournisseur</returns>
        public async Task<DemandeSite> DemandeSite(string email)
        {
            return await _context.DemandeSite
                .Where(d => d.Email == email)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cherche une demande de création de site identique à une demande.
        /// </summary>
        /// <param name="demande">DemandeSite cherchée</param>
        /// <returns>une DemandeSite qui inclut son Fournisseur avec son Site, si trouvée; null, sinon.</returns>
        public async Task<DemandeSite> DemandeSiteIdentique(DemandeSite demande)
        {
            return await _context.DemandeSite
                .Where(d => d.Email == demande.Email && d.Id == demande.Id && d.Date == demande.Date && d.Envoi == demande.Envoi)
                .Include(d => d.Fournisseur).ThenInclude(f => f.Site)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Ajoute une DemandeSite avec son Fournisseur et son Site. 
        /// </summary>
        /// <param name="ajout"></param>
        /// <param name="modelState">ModelStateDictionary où inscrire les erreurs de validation</param>
        /// <returns>un Objet contenant l'Id commun aux objets ajoutés et la Date de l'ajout.</returns>
        public async new Task<RetourDeService<DemandeSiteDate>> Ajoute(FournisseurAAjouter ajout, ModelStateDictionary modelState)
        {
            DateTime date = DateTime.Now;
            RetourDeService<Fournisseur> retourFournisseur = await base.Ajoute(ajout, modelState, date);
            if (!retourFournisseur.Ok)
            {
                return new RetourDeService<DemandeSiteDate>(retourFournisseur);
            }
            Fournisseur fournisseur = retourFournisseur.Entité;
            DemandeSite demande = new DemandeSite
            {
                Email = ajout.Email,
                Id = fournisseur.Id,
                Date = date
            };
            _context.DemandeSite.Add(demande);
            DemandeSiteDate demandeDate = new DemandeSiteDate
            {
                Id = fournisseur.Id,
                Date = date
            };
            return  await SaveChangesAsync(demandeDate);
        }

        /// <summary>
        /// Retourne la liste des DemandeSiteVue des DemandeSite enregistrées avec leurs Fournissuers.
        /// </summary>
        /// <returns></returns>
        public async Task<List<DemandeSiteVue>> Demandes()
        {
            List<DemandeSite> demandes = await _context.DemandeSite
                .Include(demande => demande.Fournisseur).ThenInclude(fournisseur => fournisseur.Site)
                .AsNoTracking()
                .ToListAsync();
            List<DemandeSiteVue> vues = demandes.Select(demande => new DemandeSiteVue(demande)).ToList();
            return vues;
        }

        /// <summary>
        /// Supprime une DemandeSite et son Fournisseur de la bdd.
        /// </summary>
        /// <param name="demande">DemandeSite à supprimer</param>
        /// <returns></returns>
        public async Task<RetourDeService> Annule(DemandeSite demande)
        {
            Fournisseur fournisseur = await _context.Fournisseur.Where(f => f.Id == demande.Id).FirstOrDefaultAsync();
            _context.Fournisseur.Remove(fournisseur);
            _context.DemandeSite.Remove(demande);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Supprime une DemandeSite mais pas son Fournisseur de la bdd.
        /// </summary>
        /// <param name="demande">DemandeSite à supprimer</param>
        /// <returns></returns>
        public async Task<RetourDeService> Supprime(DemandeSite demande)
        {
            _context.DemandeSite.Remove(demande);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Fixe la date d'envoi de la demande et envoie un message à l'Email de la demande avec un lien contenant la DemandeSite encodée.
        /// </summary>
        /// <param name="demande">DemandeSite à envoyer</param>
        /// <returns></returns>
        public async Task<RetourDeService<DemandeSiteEnvoi>> EnvoieEmailDemandeSite(DemandeSite demande)
        {
            Fournisseur fournisseur = await _context.Fournisseur
                .Where(f => f.Id == demande.Id)
                .Include(f => f.Site)
                .FirstAsync();
            string objet = "Création du site: " + fournisseur.Site.Titre;
            string urlBase = ClientApp.Url(ClientApp.NouveauSite);
            string message = "Vous pouvez finaliser la création de votre site: " + fournisseur.Site.Titre;

            demande.Envoi = DateTime.Now;
            await _emailService.EnvoieEmail<DemandeSite>(demande.Email, objet, message, urlBase, demande, null);
            _context.DemandeSite.Update(demande);
            DemandeSiteEnvoi demandeEnvoi = new DemandeSiteEnvoi
            {
               Envoi = DateTime.Now
            };
            return await SaveChangesAsync(demandeEnvoi);
        }

        /// <summary>
        /// DemandeSite contenue dans le code du lien envoyé dans le message email d'invitation.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>la DemandeSite contenue dans le code, si le code est valide; null, sinon</returns>
        public DemandeSite DécodeDemandeSite(string code)
        {
            return _emailService.DécodeCodeDeEmail<DemandeSite>(code);
        }

        /// <summary>
        /// Fixe l'UtilisateurId du Fournisseur et enregistre dans la bdd.
        /// </summary>
        /// <param name="fournisseur">Fournisseur d'une DemandeSite à activer</param>
        /// <param name="utilisateur">Utilisateur affecté à ce Fournisseur</param>
        /// <returns></returns>
        public async Task<RetourDeService> FixeUtilisateur(Fournisseur fournisseur, Utilisateur utilisateur)
        {
            fournisseur.UtilisateurId = utilisateur.Id;
            _context.Fournisseur.Update(fournisseur);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Retourne la liste des FournisseurVue des Fournissuers qui ont un Utilisateur.
        /// </summary>
        /// <returns></returns>
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

        public async Task<FournisseurVue> LitFournisseur(uint idFournisseur)
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

        public async Task<RetourDeService<RoleEtat>> ChangeEtat(Fournisseur fournisseur, EtatRole etat)
        {
            fournisseur.Etat = etat;
            _context.Fournisseur.Update(fournisseur);

            DateTime date = DateTime.Now;
            ArchiveFournisseur archive = new ArchiveFournisseur
            {
                Id = fournisseur.Id,
                Etat = etat,
                Date = date
            };
            _context.ArchiveFournisseur.Add(archive);
            return await SaveChangesAsync(RoleEtat.DeDate(date));
        }

    }
}
