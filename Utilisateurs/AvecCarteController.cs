using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sécurité.Identifiants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public class AvecCarteController : BaseController
    {
        protected readonly IUtilisateurService _utilisateurService;

        public AvecCarteController(IUtilisateurService utilisateurService)
        {
            _utilisateurService = utilisateurService;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Fixe l'Utilisateur de la carte avec ses Archives, avec ses Clients incluant leurs Archives et leur Site incluant son Fournisseur
        /// et avec ses Fournisseurs incluant leurs Archives et leur Site.
        /// </summary>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'a pas les droits requis, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteUtilisateur()
        {
            CarteUtilisateur carteUtilisateur = await _utilisateurService.CréeCarteUtilisateur(HttpContext);
            if (carteUtilisateur.Utilisateur == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Fausse carte");
                return carteUtilisateur;
            }
            if (carteUtilisateur.SessionId != carteUtilisateur.Utilisateur.SessionId)
            {
                carteUtilisateur.Erreur = Unauthorized(new { Message = "Session périmée" });
                return carteUtilisateur;
            }
            if (carteUtilisateur.Utilisateur.Etat == EtatUtilisateur.Banni)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Utilisateur banni");
                return carteUtilisateur;
            }
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que l'utilisateur est Administrateur
        /// </summary>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas administrateur actif ou nouveau, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteAdministrateur()
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }
            if (!carteUtilisateur.EstAdministrateur)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas administrateur");
            }
            return carteUtilisateur;
        }

        private async Task FixeFournisseur(CarteUtilisateur carte, Fournisseur fournisseur, PermissionsEtatRole permissions)
        {
            if (!permissions.Permet(fournisseur.Etat))
            {
                carte.Erreur = RésultatInterdit("EtatSite interdit");
                return;
            }
            carte.Fournisseur = fournisseur;
            carte.Site = fournisseur.Site;
            await carte.ArchiveDernierSite(fournisseur.Id);
        }

        private async Task FixeClient(CarteUtilisateur carte, Client client, PermissionsEtatRole permissionsFournisseur, PermissionsEtatRole permissionsClient)
        {
            if (!permissionsFournisseur.Permet(client.Site.Fournisseur.Etat))
            {
                carte.Erreur = RésultatInterdit("EtatSite interdit");
                return;
            }
            if (!permissionsClient.Permet(client.Etat))
            {
                carte.Erreur = RésultatInterdit("EtatClient interdit");
                return;
            }
            carte.Client = client;
            carte.Site = client.Site;
            await carte.ArchiveDernierSite(client.Site.Id);
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et n'est pas fermé.
        /// Fixe le Role de la carte et met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas usager actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteUsager(uint idSite, PermissionsEtatRole permissionsFournisseur, PermissionsEtatRole permissionsClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Fournisseur fournisseur = carteUtilisateur.Utilisateur.Fournisseurs.Where(f => f.Id == idSite).FirstOrDefault();
            if (fournisseur != null)
            {
                await FixeFournisseur(carteUtilisateur, fournisseur, permissionsFournisseur);
                return carteUtilisateur;
            }

            Client client = carteUtilisateur.Utilisateur.Clients.Where(c => c.SiteId == idSite).FirstOrDefault();
            if (client == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas usager");
                return carteUtilisateur;
            }
            await FixeClient(carteUtilisateur, client, permissionsFournisseur, permissionsClient);
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que l'utilisateur a un Fournisseur correspondant à l'Id du site et que son Site n'est pas fermé.
        /// Fixe le Fournisseur de la carte.
        /// Vérifie que le role est celui du fournisseur du site.
        /// Met éventuellement à jour le DernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="idSite">Id du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas fournisseur actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteFournisseur(uint idSite, PermissionsEtatRole permissionsFournisseur)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Fournisseur fournisseur = carteUtilisateur.Utilisateur.Fournisseurs.Where(f => f.Id == idSite).FirstOrDefault();

            if (fournisseur == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas fournisseur");
                return carteUtilisateur;
            }

            await FixeFournisseur(carteUtilisateur, fournisseur, permissionsFournisseur);
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au client existe et n'est pas fermé.
        /// Vérifie que le role n'est pas celui du fournisseur du site.
        /// Fixe le Role de la carte.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="idClient">Id du client</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas le client de la clé, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteClientDeClient(uint idClient, PermissionsEtatRole permissionsFournisseur, PermissionsEtatRole permissionsClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Client client = carteUtilisateur.Utilisateur.Clients
                .Where(c => c.Id == idClient)
                .FirstOrDefault();
            if (client == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas le client");
                return carteUtilisateur;
            }

            await FixeClient(carteUtilisateur, client, permissionsFournisseur, permissionsClient);
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Cherche parmi les roles de la carte un role de client ayant la key du client ou à défaut un role de fournisseur ayant la key du site
        /// Vérifie que le role trouvé n'est pas fermé.
        /// Fixe le Role de la carte.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="idClient">Id du client</param>
        /// <param name="idSite">Id du site</param>
        /// <returns></returns>
        protected async Task<CarteUtilisateur> CréeCarteClientDeClientOuFournisseurDeSite(uint idClient, uint idSite,
            PermissionsEtatRole permissionsFournisseur, PermissionsEtatRole permissionsClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Client client = carteUtilisateur.Utilisateur.Clients
                .Where(c => c.Id == idClient)
                .FirstOrDefault();
            if (client != null)
            {
                await FixeClient(carteUtilisateur, client, permissionsFournisseur, permissionsClient);
                return carteUtilisateur;
            }

            Fournisseur fournisseur = carteUtilisateur.Utilisateur.Fournisseurs
                .Where(f => f.Id == idSite)
                .FirstOrDefault();
            if (fournisseur == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Ni client ni fournisseur");
                return carteUtilisateur;
            }
            await FixeFournisseur(carteUtilisateur, fournisseur, permissionsFournisseur);
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Cherche parmi les roles de la carte un role de client ayant la key du client ou à défaut le role du fournisseur du site du client.
        /// Vérifie que le role trouvé n'est pas fermé.
        /// Fixe le Role de la carte.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="idClient">objet ayant les Uid et Rno du client</param>
        /// <returns></returns>
        protected async Task<CarteUtilisateur> CréeCarteClientOuFournisseurDeClient(uint idClient,
            PermissionsEtatRole permissionsFournisseur, PermissionsEtatRole permissionsClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Client client = carteUtilisateur.Utilisateur.Clients
                .Where(c => c.Id == idClient)
                .FirstOrDefault();
            if (client != null)
            {
                await FixeClient(carteUtilisateur, client, permissionsFournisseur, permissionsClient);
                return carteUtilisateur;
            }
            Fournisseur fournisseur = await carteUtilisateur.FournisseurDeClient(idClient);
            if (fournisseur == null)
            {
                carteUtilisateur.Erreur = NotFound();
                return carteUtilisateur;
            }
            await FixeFournisseur(carteUtilisateur, fournisseur, permissionsFournisseur);
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et est actif et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et est actif.
        /// Fixe le Role de la carte.
        /// Vérifie que le role est celui du fournisseur du site.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="idSite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas fournisseur actif du site, Conflict si le site n'est pas d'état Catalogue,
        /// Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteFournisseurCatalogue(uint idSite, PermissionsEtatRole permissionsFournisseur)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(idSite, permissionsFournisseur);
            if (carte.Erreur == null)
            {
                Site site = carte.Fournisseur.Site;
                if (site.Ouvert)
                {
                    carte.Erreur = Conflict();
                }
            }
            return carte;
        }

        protected IUtilisateurService UtilisateurService => _utilisateurService;


        protected async Task<RetourDeService<Utilisateur>> CréeUtilisateur(ICréeCompteVue vue)
        {
            RetourDeService<Utilisateur> retour = await UtilisateurService.CréeUtilisateur(vue);

            if (retour.Type == TypeRetourDeService.IdentityError)
            {
                IEnumerable<IdentityError> errors = (IEnumerable<IdentityError>)retour.Objet;
                foreach (IdentityError error in errors)
                {
                    if (error.Code == IdentityErrorCodes.DuplicateUserName)
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, "email", "nomPris");
                    }
                    else
                    {
                        ErreurDeModel.AjouteAModelState(ModelState, error.Code);
                    }
                }
            }
            return retour;

        }

        /// <summary>
        /// Connecte l'utilisateur. Si la connection a bien lieu, ajoute à la Response de la requête un header contenant le jeton identifiant.
        /// </summary>
        /// <param name="utilisateur">Utilisateur à connecter</param>
        /// <returns>un OkObjectResult contenant l'Identifiant de l'utilisateur</returns>
        protected async Task<IActionResult> Connecte(Utilisateur utilisateur)
        {
            if (!PermissionsEtatUtilisateur.PasFermé.Permet(utilisateur.Etat))
            {
                return RésultatInterdit("Cet utilisateur n'est pas autorisé");
            }
            RetourDeService retour = await _utilisateurService.Connecte(utilisateur);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(utilisateur);

            carte.SessionId = utilisateur.SessionId;
            // Ajoute à la response Ok de la requête un header contenant le jeton identifiant de la carte
            await _utilisateurService.AjouteCarteAResponse(Request.HttpContext.Response, carte);
            Identifiant identifiant = await carte.Identifiant();
            return RésultatCréé(identifiant);

        }

    }
}
