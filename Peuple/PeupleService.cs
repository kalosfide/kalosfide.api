using KalosfideAPI.Administrateurs;
using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Fournisseurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Roles;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    class RetourEnregistrement
    {
        public Administrateur Administrateur { get; set; }
        public Fournisseur Fournisseur { get; set; }
        public Client Client { get; set; }
        public Site Site { get; set; }

        public RetourDeService Retour { get; set; }

    }
    public class PeupleService: BaseService, IPeupleService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly IRoleService _roleService;
        private readonly IAdministrateurService _administrateurService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ISiteService _siteService;
        private readonly IClientService _clientService;
        private readonly ICatégorieService _catégorieService;
        private readonly IProduitService _produitService;

        public PeupleService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            IRoleService roleService,
            IAdministrateurService administrateurService,
            IFournisseurService fournisseurService,
            ISiteService siteService,
            IClientService clientService,
            ICatégorieService catégorieService,
            IProduitService produitService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _roleService = roleService;
            _administrateurService = administrateurService;
            _fournisseurService = fournisseurService;
            _siteService = siteService;
            _clientService = clientService;
            _catégorieService = catégorieService;
            _produitService = produitService;
        }

        async Task<RetourEnregistrement> Enregistre(string type, Enregistrement.VueBase vue)
        {
            RetourEnregistrement retour = new RetourEnregistrement();

            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = vue.Email,
                Email = vue.Email,
            };
            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur(applicationUser, vue.Password);
            if (!retourUtilisateur.Ok)
            {
                retour.Retour = retourUtilisateur;
                return retour;
            }
            Utilisateur utilisateur = retourUtilisateur.Objet as Utilisateur;

            try
            {
                Role role = await _roleService.CréeRole(utilisateur);
                _roleService.AjouteSansSauver(role);
                switch (type)
                {
                    case TypeDeRole.Administrateur.Code:
                        Administrateur administrateur = new Administrateur
                        {
                            Uid = role.Uid,
                            Rno = role.Rno
                        };
                        retour.Administrateur = administrateur;
                        _administrateurService.AjouteSansSauver(administrateur);
                        break;
                    case TypeDeRole.Fournisseur.Code:
                        Fournisseur fournisseur = _fournisseurService.CréeFournisseur(role, vue as Enregistrement.EnregistrementFournisseurVue);
                        _fournisseurService.AjouteSansSauver(fournisseur);
                        retour.Fournisseur = fournisseur;
                        Site site = _siteService.CréeSite(role, vue as Enregistrement.EnregistrementFournisseurVue);
                        retour.Site = site;
                        _siteService.AjouteSansSauver(site);
                        break;
                    case TypeDeRole.Client.Code:
                        Client client = _clientService.CréeClient(role, vue as Enregistrement.EnregistrementClientVue);
                        retour.Client = client;
                        _clientService.AjouteSansSauver(client);
                        break;
                    default:
                        break;
                }
                RetourDeService retourSauve = await SaveChangesAsync();

                retour.Retour = retourSauve;
                if (!retourSauve.Ok)
                {
                    await _utilisateurService.Supprime(utilisateur);
                }

                return retour;
            }
            catch (Exception)
            {
                await _utilisateurService.Supprime(utilisateur);
                throw;
            }

        }

        public async Task<bool> EstPeuplé()
        {
            return await _context.Users.AnyAsync();
        }

        public async Task<RetourDeService> Peuple()
        {
            Enregistrement.VueBase[] vues = PeuplementUtilisateurs.Fournisseurs;
            Fournisseur[] fournisseurs = new Fournisseur[vues.Length];
            Site[] sites = new Site[vues.Length];
            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
            for (int i = 0; i < vues.Length && retour.Ok; i++)
            {
                RetourEnregistrement retourEnregistrement = await Enregistre(TypeDeRole.Fournisseur.Code, vues[i]);
                fournisseurs[i] = retourEnregistrement.Fournisseur;
                sites[i] = retourEnregistrement.Site;
                retour = retourEnregistrement.Retour;
            }
            List<Client> clients = new List<Client>();
            vues = PeuplementUtilisateurs.ClientsAvecCompte;
            for (int i = 0; i < vues.Length && retour.Ok; i++)
            {
                RetourEnregistrement retourEnregistrement = await Enregistre(TypeDeRole.Client.Code, vues[i]);
                clients.Add(retourEnregistrement.Client);
                retour = retourEnregistrement.Retour;
            }
            for (int i = 0; i < sites.Length && retour.Ok; i++)
            {
                int nb = i == 0 ? 50 : 15;
                for (int no = 1; no <= nb && retour.Ok; no++)
                {
                    retour = await _clientService.Ajoute(sites[i], PeuplementUtilisateurs.Client(no));
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
