using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Utilisateurs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public class PeupleService: BaseService, IPeupleService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly IClientService _clientService;
        private readonly ICatégorieService _catégorieService;
        private readonly IProduitService _produitService;

        public PeupleService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            IClientService clientService,
            ICatégorieService catégorieService,
            IProduitService produitService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _clientService = clientService;
            _catégorieService = catégorieService;
            _produitService = produitService;
        }

        public async Task<bool> EstPeuplé()
        {
            return await _context.Users.AnyAsync();
        }

        public async Task<RetourDeService> Peuple()
        {
            EnregistrementFournisseurVue[] vues = PeuplementUtilisateurs.Fournisseurs;
            Site[] sites = new Site[vues.Length];
            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
            for (int i = 0; i < PeuplementUtilisateurs.Fournisseurs.Length && retour.Ok; i++)
            {
                EnregistrementFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[i];
                RetourDeService<ApplicationUser> retourUser = await _utilisateurService.CréeUtilisateur(vue);
                if (retourUser.Ok)
                {
                    await _utilisateurService.ConfirmeEmailDirect(retourUser.Entité);
                    RetourDeService<Role> retourRole = await _utilisateurService.CréeRoleSite(retourUser.Entité.Utilisateur.Uid, vue);
                    if (retourRole.Ok)
                    {
                        sites[i] = retourRole.Entité.Site;
                    }
                    retour = retourRole;
                }
                else
                {
                    retour = retourUser;
                }
            }
            for (int i = 0; i < PeuplementUtilisateurs.ClientsAvecCompte.Length && retour.Ok; i++)
            {
                EnregistrementClientVue vue = PeuplementUtilisateurs.ClientsAvecCompte[i];
                RetourDeService<ApplicationUser> retourUser = await _utilisateurService.CréeUtilisateur(vue);
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
                int nb = i == 0 ? 50 : 15;
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
                int nbCatégories = i == 0 ? 20 : 0;
                int nbProduits = i == 0 ? 150 : 0;
                PeuplementCatalogue catalogue = new PeuplementCatalogue(sites[i], nbCatégories, nbProduits);
                for (int j = 0; j < nbCatégories && retour.Ok; j++)
                {
                    retour = await _catégorieService.Ajoute(catalogue.Catégories.ElementAt(j));
                }
                for (int j = 0; j < nbProduits && retour.Ok; j++)
                {
                    retour = await _produitService.Ajoute(catalogue.Produits.ElementAt(j));
                }
            }

            return retour;
        }
    }
}
