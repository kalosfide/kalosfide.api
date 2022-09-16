using KalosfideAPI.Catégories;
using KalosfideAPI.CLF;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Fournisseurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Préférences;
using KalosfideAPI.Produits;
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
    public class PeupleService: BaseService, IPeupleService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ISiteService _siteService;
        private readonly IClientService _clientService;
        private readonly ICatégorieService _catégorieService;
        private readonly IProduitService _produitService;
        private readonly ICLFService _clfService;
        private readonly IPréférenceService _préférenceService;

        public PeupleService(ApplicationContext context,
            IUtilisateurService utilisateurService,
            IFournisseurService fournisseurService,
            ISiteService siteService,
            IClientService clientService,
            ICatégorieService catégorieService,
            IProduitService produitService,
            ICLFService clfService,
            IPréférenceService préférenceService
            ) : base(context)
        {
            _utilisateurService = utilisateurService;
            _fournisseurService = fournisseurService;
            _siteService = siteService;
            _clientService = clientService;
            _catégorieService = catégorieService;
            _produitService = produitService;
            _clfService = clfService;
            _préférenceService = préférenceService;
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
        /// Trouve l'Utilisateur défini par la vue s'il existe. Sinon, crée l'Utilisateur défini par la vue.
        /// </summary>
        /// <param name="compteVue"></param>
        /// <returns>un RetourDeService<Utilisateur> contenant </returns>
        private async Task<RetourDeService<Utilisateur>> Utilisateur(ICréeCompteVue compteVue)
        {
            Utilisateur utilisateur = await _utilisateurService.UtilisateurDeEmail(compteVue.Email);
            if (utilisateur != null)
            {
                Utilisateur vérifié = await _utilisateurService.UtilisateurVérifié(compteVue.Email, compteVue.Password);
                if (vérifié == null)
                {
                    // il y a déjà un utilisateur avec l'email de la vue mais un mot de passe qui n'est pas celui de la vue
                    return new RetourDeService<Utilisateur>(TypeRetourDeService.ModelError);
                }
                return new RetourDeService<Utilisateur>(vérifié);
            }
            RetourDeService<Utilisateur> retourUtilisateur = await _utilisateurService.CréeUtilisateur(compteVue);
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
            RetourDeService<Utilisateur> retourUtilisateur = await Utilisateur(vue);
            if (!retourUtilisateur.Ok)
            {
                return retourUtilisateur;
            }
            Utilisateur utilisateur = retourUtilisateur.Entité;
            Fournisseur fournisseur = new Fournisseur
            {
                Id = id,
                UtilisateurId = utilisateur.Id,
                Etat = EtatRole.Actif,
                Siret = "légal" + id,
                Site = new Site
                {
                    Id = id,
                    Ouvert = false
                }
            };
            Role.CopieData(vue, fournisseur);
            Site.CopieData(vue, fournisseur.Site);
            RetourDeService<Fournisseur> retourFournisseur = await _fournisseurService.AjouteSansValider(fournisseur);
            if (!retourFournisseur.Ok)
            {
                return retourFournisseur;
            }
            await _utilisateurService.FixeIdDernierSite(utilisateur, id);
            peupleId.Fournisseur = id;

            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);

            PeuplementCatalogue catalogue = null;
            if (vue.Produits.HasValue)
            {
                retour = await _préférenceService.Ajoute(new Préférence { Id = PréférenceId.UsageCatégories, SiteId = id, UtilisateurId = "tous", Valeur = "2" });
                if (!retour.Ok)
                {
                    return retour;
                }
                DateTime dateDébut = DateTime.Now;
                int nbProduits = vue.Produits.Value;
                int nbCatégories = vue.Catégories ?? 1;
                catalogue = new PeuplementCatalogue(fournisseur.Id, nbCatégories, nbProduits, peupleId);
                for (int j = 0; j < nbCatégories && retour.Ok; j++)
                {
                    retour = await _catégorieService.AjouteSansValider(catalogue.Catégories.ElementAt(j));
                }

                for (int j = 0; j < nbProduits && retour.Ok; j++)
                {
                    retour = await _produitService.AjouteSansValider(catalogue.Produits.ElementAt(j));
                }
                DateTime dateFin = DateTime.Now;
                await _produitService.TermineModification(fournisseur.Id, dateDébut, dateFin);
                await _catégorieService.TermineModification(fournisseur.Id, dateDébut, dateFin);
                if (nbProduits > 0)
                {
                    retour = await _siteService.TermineEtatCatalogue(fournisseur.Site, dateFin);
                }
            }

            List<Client> clients = new List<Client>();
            if (vue.Clients != null)
            {
                for (int i = 0; i < vue.Clients.Length && retour.Ok; i++)
                {
                    retour = await AjouteClient(vue.Clients[i], peupleId, clients);
                }
            }
            if (vue.ClientsSansCompte.HasValue)
            {
                for (int i = 0; i < vue.ClientsSansCompte.Value && retour.Ok; i++, id++)
                {
                    retour = await AjouteClient(peupleId);
                }
            }

            if (!retour.Ok || catalogue == null || clients.Count == 0)
            {
                return retour;
            }

            Client client = clients[0];
            retour = await _clientService.Active(client);
            if (!retour.Ok)
            {
                return retour;
            }
            uint noBon = 1;
            List<DocCLF> commandes = new List<DocCLF>();
            RetourDeService<DocCLF> retourBon = await AjouteCommande(client.Id, catalogue.Produits, noBon++, fournisseur.Site);
            if (!retourBon.Ok)
            {
                return new RetourDeService(retourBon);
            }
            commandes.Add(retourBon.Entité);
            retour = await ModifieCatalogue(catalogue.Produits, fournisseur.Site);
            return retour;
        }

        /// <summary>
        /// Crée un Client avec son Utilisateur.
        /// </summary>
        /// <param name="vue">PeupleClientVue définissant le Client à créer</param>
        /// <param name="peupleId">PeupleId contenant les Id des derniers objets créés</param>
        /// <returns></returns>
        private async Task<RetourDeService> AjouteClient(PeupleClientVue vue, PeupleId peupleId, List<Client> clients)
        {
            uint id = peupleId.Client + 1;
            RetourDeService<Utilisateur> retourUtilisateur = await Utilisateur(vue);
            if (!retourUtilisateur.Ok)
            {
                return retourUtilisateur;
            }
            Utilisateur utilisateur = retourUtilisateur.Entité;
            Client client = new Client
            {
                Id = id,
                UtilisateurId = utilisateur.Id,
                SiteId = peupleId.Fournisseur,
                Etat = EtatRole.Nouveau
            };
            Role.CopieData(vue, client);
            RetourDeService retour = await _clientService.AjouteSansValider(client);
            if (retour.Ok)
            {
                peupleId.Client = id;
                clients.Add(client);
                await _utilisateurService.FixeIdDernierSite(utilisateur, client.SiteId);
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
            RetourDeService retour = await _clientService.AjouteSansValider(client);
            if (retour.Ok)
            {
                peupleId.Client = id;
            }
            return retour;
        }

        private async Task<RetourDeService> ModifieCatalogue(List<Produit> produits, Site site)
        {
            RetourDeService retour = await _siteService.CommenceEtatCatalogue(site);
            DateTime dateDébut = DateTime.Now;
            int nbProduits = produits.Count;

            Hasard<decimal> augmentations = new Hasard<decimal>(new List<ItemAvecPoids<decimal>>
            {
                new ItemAvecPoids<decimal>(0, 40),
                new ItemAvecPoids<decimal>(10, 30),
                new ItemAvecPoids<decimal>(20, 30),
                new ItemAvecPoids<decimal>(-10, 10)
                });
            Hasard<bool> rendIndispo = new Hasard<bool>(new List<ItemAvecPoids<bool>>
            {
                new ItemAvecPoids<bool>(true, 1),
                new ItemAvecPoids<bool>(false, 9)
                });
            Hasard<bool> rendDispo = new Hasard<bool>(new List<ItemAvecPoids<bool>>
            {
                new ItemAvecPoids<bool>(true, 2),
                new ItemAvecPoids<bool>(false, 1)
                });

            for (int j = 0; j < nbProduits && retour.Ok; j++)
            {
                Produit produit = await _produitService.Lit(produits.ElementAt(j).Id);
                decimal? prix = null;
                decimal augmentation = augmentations.Suivant();
                if (augmentation != 0)
                {
                    prix = produit.Prix * (1m + augmentation / 100m);
                }
                bool? disponible = null;
                if (produit.Disponible)
                {
                    bool change = rendIndispo.Suivant();
                    if (change)
                    {
                        disponible = false;
                    }
                }
                else
                {
                    bool change = rendDispo.Suivant();
                    if (change)
                    {
                        disponible = true;
                    }

                }

                if (prix.HasValue || disponible.HasValue)
                {
                    ProduitAEditer produitAEditer = new ProduitAEditer
                    {
                        Prix = prix,
                        Disponible = disponible
                    };
                    retour = await _produitService.Edite(produit, produitAEditer);

                }
            }
            DateTime dateFin = DateTime.Now;
            await _produitService.TermineModification(site.Id, dateDébut, dateFin);
            if (nbProduits > 0)
            {
                retour = await _siteService.TermineEtatCatalogue(site, dateFin);
            }
            return retour;
        }

        private async Task<RetourDeService<DocCLF>> AjouteCommande(uint idClient, List<Produit> produits, uint no, Site site)
        {
            RetourDeService<DocCLF> retourBon = new RetourDeService<DocCLF>(TypeRetourDeService.Ok);
            List<Produit> disponibles = produits.Where(p => p.Disponible).ToList();
            if (disponibles.Count == 0)
            {
                return retourBon;
            }
            retourBon = await _clfService.AjouteBon(idClient, TypeCLF.Commande, no);
            if (retourBon.Ok)
            {
                RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
                DocCLF docCLF = retourBon.Entité;
                for (int i = 0; i < disponibles.Count && retour.Ok; i++)
                {
                    Produit produit = disponibles[i];
                    CLFLigne ligne = new CLFLigne
                    {
                        Id = idClient,
                        No = no,
                        ProduitId = produit.Id,
                        Quantité = 10,
                    };
                    retour = await _clfService.AjouteLigneCommande(produit, ligne);
                }
                if (retour.Ok)
                {
                    retour = await _clfService.EnvoiCommande(site, docCLF);
                }
                else
                {
                    retourBon = new RetourDeService<DocCLF>(retour);
                }
            }
            return retourBon;

        }

        private async Task<RetourDeService> AjouteLivraison(uint idClient, List<Produit> produits, uint no, Site site)
        {
            RetourDeService retour = new RetourDeService(TypeRetourDeService.Ok);
            List<Produit> disponibles = produits.Where(p => p.Disponible).ToList();
            if (disponibles.Count == 0)
            {
                return retour;
            }
            RetourDeService<DocCLF> retourBon = await _clfService.AjouteBon(idClient, TypeCLF.Commande, no);
            if (retourBon.Ok)
            {
                DocCLF docCLF = retourBon.Entité;
                for (int i = 0; i < disponibles.Count && retour.Ok; i++)
                {
                    Produit produit = disponibles[i];
                    CLFLigne ligne = new CLFLigne
                    {
                        Id = idClient,
                        No = no,
                        ProduitId = produit.Id,
                        Quantité = 10,
                    };
                    retour = await _clfService.AjouteLigneCommande(produit, ligne);
                }
                if (retour.Ok)
                {
                    retour = await _clfService.EnvoiCommande(site, docCLF);
                }
            }
            else
            {
                retour = new RetourDeService(retourBon);
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
