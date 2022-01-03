using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Sites;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    public class FournisseurController: AvecIdUintController<Fournisseur, FournisseurAAjouter, FournisseurAEditer>
    {
        private readonly IEnvoieEmailService _emailService;
        private readonly ISiteService _siteService;

        public FournisseurController(IFournisseurService service,
            IUtilisateurService utilisateurService,
            ISiteService siteService,
            IEnvoieEmailService emailService
            ) : base(service, utilisateurService)
        {
            _siteService = siteService;
            _emailService = emailService;
        }
        private IFournisseurService _service { get => __service as IFournisseurService; }

        /// <summary>
        /// Ajoute à la bdd un Fournisseur avec son Site sans Utilisateur.
        /// Enregistre une demande de création d'un nouveau site.
        /// </summary>
        /// <param name="ajout">FournisseurAAjouter définissant le site à créer</param>
        /// <returns></returns>
        [HttpPost("demande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Demande(FournisseurAAjouter ajout)
        {
            DemandeSite demande = await _service.DemandeSite(ajout.Email);
            if (demande != null)
            {
                return RésultatBadRequest("Il y a déjà une demande de création de site pour cet email.");
            }

            VérifieSansEspacesData(ajout, Fournisseur.AvérifierSansEspacesData);
            VérifieSansEspacesData(ajout.Site, Site.AvérifierSansEspacesData);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return await base.Ajoute(ajout);
        }

        /// <summary>
        /// Cherche une DemandeSite à partir de son Email.
        /// Envoie un message email à cette adresse avec un lien contenant la DemandeSite encodée.
        /// Enregistre la date d'envoi dans la bdd.
        /// </summary>
        /// <param name="email">Email de la demande cherchée</param>
        /// <returns></returns>
        [HttpPost("invite")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Invite([FromQuery] string email)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            DemandeSite demande = await _service.DemandeSite(email);
            if (demande == null)
            {
                return NotFound();
            }

            RetourDeService retour = await _service.EnvoieEmailDemandeSite(demande);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }
            return RésultatCréé(demande);
        }

        /// <summary>
        /// Retourne les informations sur le Fournisseur si le code correspond à une DemandeSite enregistrée
        /// pour pouvoir les afficher dans la page ActiveSite
        /// </summary>
        /// <param name="code">code du lien du message email d'invitation</param>
        /// <returns></returns>
        [HttpGet("demande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Demande([FromQuery] string code)
        {

            DemandeSite demande = _service.DécodeDemandeSite(code);
            if (demande == null)
            {
                return RésultatBadRequest("Pas de demande dans le code.");
            }

            DemandeSite enregistrée = await _service.DemandeSiteIdentique(demande);
            // il doit y avoir une demande enregistrée identique à la demande décodée
            if (enregistrée == null)
            {
                return NotFound();
            }
            return Ok(enregistrée.Fournisseur);
        }

        /// <summary>
        /// Ajoute à la bdd un Utilisateur pour le Fournisseur d'une DemandeSite.
        /// </summary>
        /// <param name="àActiver">DemandSiteAActiver qui contient un code contenant la DemandeSite originale
        /// et les coordonnées de connection</param>
        /// <returns></returns>
        [HttpPost("active")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [AllowAnonymous]
        public async Task<IActionResult> Active(DemandSiteAActiver àActiver)
        {
            DemandeSite demande = _service.DécodeDemandeSite(àActiver.Code);
            if (demande == null)
            {
                return RésultatBadRequest("Pas de demande dans le code.");
            }

            DemandeSite enregistrée = await _service.DemandeSiteIdentique(demande);
            // il doit y avoir une demande enregistrée identique à la demande décodée
            if (enregistrée == null)
            {
                return NotFound();
            }

            // si les données définissant le Fournisseur et son Site n'ont pas été modifiées, c'est une activation
            if (àActiver.Fournisseur == null && àActiver.Site == null)
            {
                // on cherche si l'Email de la demande correspond à un Utilisateur existant.
                Utilisateur utilisateur = await UtilisateurService.UtilisateurDeEmail(àActiver.Email);
                if (utilisateur == null)
                {
                    // l'invité n'a pas de compte Kalosfide
                    //on crée l'Utilisateur
                    RetourDeService<Utilisateur> retourUser = await CréeUtilisateur(àActiver);
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    if (!retourUser.Ok)
                    {
                        return SaveChangesActionResult(retourUser);
                    }
                    utilisateur = retourUser.Entité;
                }
                else
                {
                    // l'utilisateur a un compte Kalosfide
                    // on vérifie le mot de passe
                    utilisateur = await UtilisateurService.UtilisateurVérifié(àActiver.Email, àActiver.Password);
                    if (utilisateur == null)
                    {
                        return RésultatBadRequest("Nom ou mot de passe invalide");
                    }
                    // l'utilisateur doit être actif
                    if(utilisateur.Etat != EtatUtilisateur.Actif)
                    {
                        return RésultatInterdit("Utilisateur non actif");
                    }
                }
                // on lie le Fournisseur à l'utilisateur
                RetourDeService retour = await _service.FixeUtilisateur(enregistrée.Fournisseur, utilisateur);
                if (!retour.Ok)
                {
                    return SaveChangesActionResult(retour);
                }

                // Supprime la DemandeSite de la table
                await _service.Supprime(enregistrée);

                // confirme l'email si ce n'est pas fait
                if (!utilisateur.EmailConfirmed)
                {
                    await UtilisateurService.ConfirmeEmailDirect(utilisateur);
                }

                return await Connecte(utilisateur);
            }

            if (àActiver.Fournisseur != null)
            {
                VérifieSansEspacesDataAnnulable(àActiver.Fournisseur, Fournisseur.AvérifierSansEspacesDataAnnulable);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // il faut éditer le Fournisseur
                RetourDeService retour = await _service.Edite(enregistrée.Fournisseur, àActiver.Fournisseur);
                if (!retour.Ok)
                {
                    return SaveChangesActionResult(retour);
                }
            }
            if (àActiver.Site != null)
            {
                VérifieSansEspacesDataAnnulable(àActiver.Site, Site.AvérifierSansEspacesDataAnnulable);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // il faut éditer le Site
                RetourDeService retour = await _siteService.Edite(enregistrée.Fournisseur.Site, àActiver.Site);
                if (!retour.Ok)
                {
                    return SaveChangesActionResult(retour);
                }
            }

            RetourDeService retourEmail = await _service.EnvoieEmailDemandeSite(demande);
            if (!retourEmail.Ok)
            {
                return SaveChangesActionResult(retourEmail);
            }
            return RésultatCréé(demande);
        }

        /// <summary>
        /// Ramène la bdd à l'état où elle était avant qu'une demande soit enregistrée.
        /// Supprime la DemandeSite et son Fournisseur.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpDelete("demande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> SupprimeDemande([FromQuery] string email)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            // on cherche si une DemandeSite a été enregistrée
            DemandeSite demande = await _service.DemandeSite(email);
            if (demande == null)
            {
                return NotFound();
            }
            // la DemandeSite à supprimer n'inclut pas son Fournisseur
            RetourDeService retour = await _service.Annule(demande);
            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Change l'état d'un Fournisseur.
        /// </summary>
        /// <param name="id">Id d'un Fournisseur</param>
        /// <param name="etat">nouvel EtatRole</param>
        /// <returns></returns>
        [HttpPost("etat")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorized
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Etat([FromQuery] uint id, EtatRole etat)
        {
            CarteUtilisateur carte = await CréeCarteAdministrateur();
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }
            Fournisseur fournisseur = await _service.Lit(id);
            if (fournisseur == null)
            {
                return NotFound();
            }

            // le nouvel état doit être différent de l'actuel et de EtatRole.Fermé
            // le passage à l'état fermé se faisant automatiquement après un temps d'inactivité fixé
            if (etat == fournisseur.Etat || etat == EtatRole.Fermé)
            {
                return RésultatBadRequest("Nouvel état incorrect");
            }

            RetourDeService<RoleEtat> retour = await _service.ChangeEtat(fournisseur, etat);
            if (!retour.Ok)
            {
                return SaveChangesActionResult(retour);
            }

            return RésultatCréé(retour.Entité);
        }
    }
}
