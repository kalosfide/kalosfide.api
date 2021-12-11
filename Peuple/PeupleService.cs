using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public class PeupleService: BaseService, IPeupleService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly ISiteService _siteService;
        private readonly IClientService _clientService;
        private readonly ICatégorieService _catégorieService;
        private readonly IProduitService _produitService;

        public PeupleService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            ISiteService siteService,
            IClientService clientService,
            ICatégorieService catégorieService,
            IProduitService produitService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _siteService = siteService;
            _clientService = clientService;
            _catégorieService = catégorieService;
            _produitService = produitService;
        }

        public async Task<bool> EstPeuplé()
        {
            PeupleFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[0];
            ApplicationUser user = await _utilisateurService.ApplicationUserDeEmail(vue.Email);
            return user != null;
        }

        public async Task<RetourDeService> Peuple()
        {
            RetourDeService<ApplicationUser> retourUser;

            // Crée les fournisseurs: Utilisateur, Role, Site
            PeupleFournisseurVue[] vues = PeuplementUtilisateurs.Fournisseurs;
            Site[] sites = new Site[vues.Length];
            Utilisateur[] utilisateursFournisseurs = new Utilisateur[vues.Length];
            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
            for (int i = 0; i < PeuplementUtilisateurs.Fournisseurs.Length; i++)
            {
                PeupleFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[i];
                retourUser = await _utilisateurService.CréeUtilisateur(vue);
                if (!retourUser.Ok)
                {
                    return retourUser;
                }
                await _utilisateurService.ConfirmeEmailDirect(retourUser.Entité);
                utilisateursFournisseurs[i] = retourUser.Entité.Utilisateur;
                RetourDeService<Role> retourRole = await _siteService.CréeRoleSite(retourUser.Entité.Utilisateur, vue);
                if (!retourRole.Ok)
                {
                    return retourRole;
                }
                sites[i] = retourRole.Entité.Site;
            }

            if (PeuplementUtilisateurs.Fournisseurs.Length >= 2)
            {
                // Crée pour chaque fournisseur sauf le premier un Role de client des autres sites
                for (int i = 1; i < PeuplementUtilisateurs.Fournisseurs.Length; i++)
                {
                    Utilisateur utilisateur = utilisateursFournisseurs[i];
                    PeupleFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[i];
                    for (int j = 0; j < PeuplementUtilisateurs.Fournisseurs.Length; j++)
                    {
                        if (i != j)
                        {
                            retour = await _clientService.Ajoute(utilisateur, sites[j], vue);
                            if (!retour.Ok)
                            {
                                return retour;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < PeuplementUtilisateurs.ClientsAvecCompte.Length && retour.Ok; i++)
            {
                PeupleClientVue vue = PeuplementUtilisateurs.ClientsAvecCompte[i];
                retourUser = await _utilisateurService.CréeUtilisateur(vue);
                if (retourUser.Ok)
                {
                    await _utilisateurService.ConfirmeEmailDirect(retourUser.Entité);
                    Utilisateur utilisateur = retourUser.Entité.Utilisateur;
                    KeyUidRno keySite = new KeyUidRno
                    {
                        Uid = vue.SiteUid,
                        Rno = vue.SiteRno
                    };
                    retour = await _clientService.Ajoute(utilisateur, keySite, vue);
                }
                else
                {
                    retour = retourUser;
                }
            }
            for (int i = 0; i < sites.Length && retour.Ok; i++)
            {
                int nb = i == 0 ? 5 : 0;
                for (int no = 1; no <= nb && retour.Ok; no++)
                {
                    RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur();
                    if (!retourUtilisateur.Ok)
                    {
                        return retourUtilisateur;
                    }
                    retour = await _clientService.Ajoute(retourUtilisateur.Entité, sites[i], PeuplementUtilisateurs.Client(no));
                }
            }

            for (int i = 0; i < sites.Length && retour.Ok; i++)
            {
                int nbCatégories = i == 0 ? 3 : 0;
                int nbProduits = i == 0 ? 6 : 0;

                Site site = sites[i];
                PeuplementCatalogue catalogue = new PeuplementCatalogue(site, nbCatégories, nbProduits);
                for (int j = 0; j < nbCatégories && retour.Ok; j++)
                {
                    retour = await _catégorieService.Ajoute(catalogue.Catégories.ElementAt(j));
                }
                
                for (int j = 0; j < nbProduits && retour.Ok; j++)
                {
                    retour = await _produitService.Ajoute(catalogue.Produits.ElementAt(j));
                }
                DateTime maintenant = DateTime.Now;
                await _produitService.TermineModification(site, maintenant);
                await _catégorieService.TermineModification(site, maintenant);
                if (nbProduits > 0)
                {
                    retour = await _siteService.TermineEtatCatalogue(site, maintenant);
                }
            }
            // Crée l'administrateur
            CréeCompteVue adminVue = new CréeCompteVue
            {
                Email = "admin@kalosfide.fr",
                Password = "123456"
            };
            retourUser = await _utilisateurService.CréeUtilisateur(adminVue);
            if (!retourUser.Ok)
            {
                return retourUser;
            }
            await _utilisateurService.ConfirmeEmailDirect(retourUser.Entité);

            return retour;
        }
    }
}
