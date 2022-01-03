using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Fournisseurs;
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
        private readonly IFournisseurService _fournisseurService;
        private readonly ISiteService _siteService;
        private readonly IClientService _clientService;
        private readonly ICatégorieService _catégorieService;
        private readonly IProduitService _produitService;

        public PeupleService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            IFournisseurService fournisseurService,
            ISiteService siteService,
            IClientService clientService,
            ICatégorieService catégorieService,
            IProduitService produitService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _fournisseurService = fournisseurService;
            _siteService = siteService;
            _clientService = clientService;
            _catégorieService = catégorieService;
            _produitService = produitService;
        }

        public async Task<bool> EstPeuplé()
        {
            PeupleFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[0];
            Utilisateur user = await _utilisateurService.UtilisateurDeEmail(vue.Email);
            return user != null;
        }

        /// <summary>
        /// Crée l'administrateur.
        /// </summary>
        /// <returns></returns>
        private async Task<RetourDeService> AjouteAdministrateur()
        {
            CréeCompteVue vue = new CréeCompteVue
            {
                Email = "admin@kalosfide.fr",
                Password = "123456"
            };
            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur(vue);
            if (retourUtilisateur.Ok)
            {
                await _utilisateurService.ConfirmeEmailDirect(retourUtilisateur.Entité);
            }
            return retourUtilisateur;
        }

        /// <summary>
        /// Crée un Fournisseur avec son Utilisateur, son Site et l'ajoute au Peuplement.
        /// Crée éventuellement les Clients sans Utilisateur du Site et les ajoute au Peuplement.
        /// Crée éventuellement les Catégories et les Produits du Site et les compte dans le Peuplement.
        /// </summary>
        /// <param name="vue">PeupleFournisseurVue définissant le Fournisseur à créer</param>
        /// <param name="peupleId">PeupleId contenant les Id des derniers objets créés</param>
        /// <returns></returns>
        private async Task<RetourDeService> AjouteFournisseur(PeupleFournisseurVue vue, PeupleId peupleId)
        {
            uint id = peupleId.Fournisseur + 1;
            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur(vue);
            if (!retourUtilisateur.Ok)
            {
                return retourUtilisateur;
            }
            Utilisateur utilisateur = retourUtilisateur.Entité;
            await _utilisateurService.ConfirmeEmailDirect(utilisateur);
            Fournisseur fournisseur = new Fournisseur
            {
                Id = id,
                UtilisateurId = utilisateur.Id,
                Siret = "légal" + id,
                Site = new Site
                {
                    Ouvert = false
                }
            };
            Role.CopieData(vue, fournisseur);
            Site.CopieData(vue, fournisseur.Site);
            RetourDeService<Fournisseur> retourFournisseur = await _fournisseurService.Ajoute(fournisseur);
            if (!retourFournisseur.Ok)
            {
                return retourFournisseur;
            }
            fournisseur = retourFournisseur.Entité;
            fournisseur.Utilisateur = utilisateur;
            peupleId.Fournisseur = id;

            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
            if (vue.Clients != null)
            {
                for (int i = 0; i < vue.Clients.Length && retour.Ok; i++)
                {
                    retour = await AjouteClient(vue.Clients[i], peupleId);
                }
            }
            if (vue.ClientsSansCompte.HasValue)
            {
                for (int i = 0; i < vue.ClientsSansCompte.Value && retour.Ok; i++, id++)
                {
                    retour = await AjouteClient(peupleId);
                }

            }
            if (vue.Produits.HasValue)
            {
                DateTime dateDébut = DateTime.Now;
                int nbProduits = vue.Produits.Value;
                int nbCatégories = vue.Catégories ?? 1;
                PeuplementCatalogue catalogue = new PeuplementCatalogue(fournisseur.Id, nbCatégories, nbProduits, peupleId);
                for (int j = 0; j < nbCatégories && retour.Ok; j++)
                {
                    retour = await _catégorieService.Ajoute(catalogue.Catégories.ElementAt(j));
                }

                for (int j = 0; j < nbProduits && retour.Ok; j++)
                {
                    retour = await _produitService.Ajoute(catalogue.Produits.ElementAt(j));
                }
                DateTime dateFin = DateTime.Now;
                await _produitService.TermineModification(fournisseur.Id, dateDébut, dateFin);
                await _catégorieService.TermineModification(fournisseur.Id, dateDébut, dateFin);
                if (nbProduits > 0)
                {
                    retour = await _siteService.TermineEtatCatalogue(fournisseur.Site, dateFin);
                }
            }
            return retour;
        }

        /// <summary>
        /// Crée un Client avec son Utilisateur.
        /// </summary>
        /// <param name="vue">PeupleClientVue définissant le Client à créer</param>
        /// <param name="peupleId">PeupleId contenant les Id des derniers objets créés</param>
        /// <returns></returns>
        private async Task<RetourDeService> AjouteClient(PeupleClientVue vue, PeupleId peupleId)
        {
            uint id = peupleId.Client + 1;
            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur(vue);
            if (!retourUtilisateur.Ok)
            {
                return retourUtilisateur;
            }
            Utilisateur utilisateur = retourUtilisateur.Entité;
            await _utilisateurService.ConfirmeEmailDirect(utilisateur);
            Client client = new Client
            {
                Id = id,
                UtilisateurId = utilisateur.Id,
                SiteId = peupleId.Fournisseur,
                Etat = EtatRole.Nouveau
            };
            Role.CopieData(vue, client);
            RetourDeService retour = await _clientService.Ajoute(client);
            if (retour.Ok)
            {
                peupleId.Client = id;
            }
            return retour;
        }

        /// <summary>
        /// Crée un Client sans Utilisateur.
        /// </summary>
        /// <param name="peupleId">PeupleId contenant les Id des derniers objets créés</param>
        /// <returns></returns>
        private async Task<RetourDeService> AjouteClient(PeupleId peupleId)
        {
            uint id = peupleId.Client + 1;
            Client client = new Client
            {
                Id = id,
                SiteId = peupleId.Fournisseur,
                Nom = "Client" + id,
                Adresse = "Adresse" + id,
                Ville = "Ville" + id,
                Etat = EtatRole.Actif
            };
            RetourDeService retour = await _clientService.Ajoute(client);
            if (retour.Ok)
            {
                peupleId.Client = id;
            }
            return retour;
        }

        public async Task<RetourDeService> Peuple()
        {
            PeupleId peupleId = new PeupleId();
            RetourDeService retour = await AjouteAdministrateur();

            // Crée les fournisseurs: Utilisateur, Fournisseur, Site
            for (int i = 0; i < PeuplementUtilisateurs.Fournisseurs.Length && retour.Ok; i++)
            {
                PeupleFournisseurVue vue = PeuplementUtilisateurs.Fournisseurs[i];
                retour = await AjouteFournisseur(vue, peupleId);
            }

            return retour;
        }
    }
}
