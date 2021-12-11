using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using Microsoft.AspNetCore.Mvc;
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
        /// La carte contient l'Utilisateur avec ses Roles incluant leurs Sites.
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
            if (carteUtilisateur.Utilisateur.Etat == TypeEtatUtilisateur.Banni)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Utilisateur inactivé");
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

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et n'est pas fermé.
        /// Fixe le Role (incluant son Site) de la carte.
        /// </summary>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas usager actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        private async Task<CarteUtilisateur> CréeCarteUsagerBase(IKeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Role role = carteUtilisateur.Utilisateur.Roles
                    .Where(r => r.SiteUid == keySite.Uid && r.SiteRno == keySite.Rno)
                    .FirstOrDefault();
            if (role == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas usager");
                return carteUtilisateur;
            }
            if (role.Etat == TypeEtatRole.Fermé)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Usager inactivé");
                return carteUtilisateur;
            }
            carteUtilisateur.Role = role;
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et n'est pas fermé.
        /// Fixe le Role de la carte et met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas usager actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteUsager(IKeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUsagerBase(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }
            await carteUtilisateur.FixeNoDernierRole();
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et n'est pas fermé.
        /// Fixe le Role de la carte.
        /// Vérifie que le role est celui du fournisseur du site.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas fournisseur actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteFournisseur(IKeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUsagerBase(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }
            if (carteUtilisateur.Role.Uid != keySite.Uid || carteUtilisateur.Role.Rno != keySite.Rno)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas fournisseur");
                return carteUtilisateur;
            }
            await carteUtilisateur.FixeNoDernierRole();
            return carteUtilisateur;
        }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et n'est pas banni et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et n'est pas fermé.
        /// Fixe le Role de la carte.
        /// Vérifie que le role n'est pas celui du fournisseur du site.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas client actif ou nouveau du site, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteClient(IKeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUsagerBase(keySite);
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }
            if (carteUtilisateur.Role.Uid == keySite.Uid && carteUtilisateur.Role.Rno == keySite.Rno)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Client fournisseur");
                return carteUtilisateur;
            }
            await carteUtilisateur.FixeNoDernierRole();
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
        /// <param name="keyClient">objet ayant les Uid et Rno du client</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas le client de la clé, Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteClientDeClient(IKeyUidRno keyClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Role role = carteUtilisateur.Utilisateur.Roles
                .Where(r => r.Uid == keyClient.Uid && r.Rno == keyClient.Rno)
                .FirstOrDefault();
            if (role == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Pas le client");
                return carteUtilisateur;
            }
            if (role.Etat == TypeEtatRole.Fermé)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Usager inactivé");
                return carteUtilisateur;
            }
            if (Role.EstFournisseur(role))
            {
                carteUtilisateur.Erreur = RésultatInterdit("Client fournisseur");
                return carteUtilisateur;
            }
            carteUtilisateur.Role = role;
            await carteUtilisateur.FixeNoDernierRole();
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
        /// <param name="keyClient">objet ayant les Uid et Rno du client</param>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns></returns>
        protected async Task<CarteUtilisateur> CréeCarteClientDeClientOuFournisseurDeSite(IKeyUidRno keyClient, IKeyUidRno keySite)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Role role = carteUtilisateur.Utilisateur.Roles
                .Where(r => r.Uid == keyClient.Uid && r.Rno == keyClient.Rno)
                .FirstOrDefault();
            if (role != null && Role.EstFournisseur(role))
            {
                carteUtilisateur.Erreur = RésultatInterdit("Client fournisseur");
                return carteUtilisateur;
            }
            if (role == null)
            {
                role = carteUtilisateur.Utilisateur.Roles
                    .Where(r => r.Uid == keySite.Uid && r.Rno == keySite.Rno)
                    .FirstOrDefault();
            }
            if (role == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Ni client ni fournisseur");
                return carteUtilisateur;
            }
            if (role.Etat == TypeEtatRole.Fermé)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Usager inactivé");
                return carteUtilisateur;
            }
            carteUtilisateur.Role = role;
            await carteUtilisateur.FixeNoDernierRole();
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
        /// <param name="keyClient">objet ayant les Uid et Rno du client</param>
        /// <returns></returns>
        protected async Task<CarteUtilisateur> CréeCarteClientOuFournisseurDeClient(IKeyUidRno keyClient)
        {
            CarteUtilisateur carteUtilisateur = await CréeCarteUtilisateur();
            if (carteUtilisateur.Erreur != null)
            {
                return carteUtilisateur;
            }

            Role client = await _utilisateurService.RoleDeKey(keyClient);
            if (client == null)
            {
                carteUtilisateur.Erreur = NotFound();
                return carteUtilisateur;
            }
            if (Role.EstFournisseur(client))
            {
                carteUtilisateur.Erreur = RésultatInterdit("Client fournisseur");
                return carteUtilisateur;
            }

            Role role = carteUtilisateur.Utilisateur.Roles
                .Where(r => r.Uid == keyClient.Uid && r.Rno == keyClient.Rno)
                .FirstOrDefault();
            if (role == null)
            {
                role = carteUtilisateur.Utilisateur.Roles
                    .Where(r => r.Uid == client.SiteUid && r.Rno == client.SiteRno)
                    .FirstOrDefault();
            }
            if (role == null)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Ni client ni fournisseur");
                return carteUtilisateur;
            }
            if (role.Etat == TypeEtatRole.Fermé)
            {
                carteUtilisateur.Erreur = RésultatInterdit("Usager inactivé");
                return carteUtilisateur;
            }
            carteUtilisateur.Role = role;
            await carteUtilisateur.FixeNoDernierRole();
            return carteUtilisateur;
        }

        /// <summary>
        /// Ajoute un header contenant le jeton identifiant de la carte à la response Ok d'une requête de connection
        /// </summary>
        /// <param name="carte"></param>
        /// <returns>un OkObjectResult contenant l'identifiant de la carte</returns>
        protected async Task<IActionResult> ResultAvecCarte(CarteUtilisateur carte)
        {
            await _utilisateurService.AjouteCarteAResponse(Request.HttpContext.Response, carte);
            Identifiant identifiant = await carte.Identifiant();
            return new OkObjectResult(identifiant);
        }

        protected IUtilisateurService UtilisateurService => _utilisateurService;
    }
}
