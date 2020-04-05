using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Factures
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class FactureController : BaseController
    {
        private readonly IFactureService _service;
        private readonly IUtileService _utile;
        private readonly IUtilisateurService _utilisateurService;
        private readonly IClientService _clientService;

        public FactureController(IFactureService service,
            IUtileService utile,
            IUtilisateurService utilisateurService,
            IClientService clientService)
        {
            _service = service;
            _utile = utile;
            _utilisateurService = utilisateurService;
            _clientService = clientService;
        }

        #region Lecture

        /// <summary>
        /// Retourne les liste des commandes livrées non facturées regroupées par client et la liste des No et Date des livraisons de ces commandes
        /// </summary>
        /// <param name="keySite">key du fournisseur</param>
        /// <returns></returns>
        [HttpGet("/api/facture/clients")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Clients([FromQuery] KeyUidRno keySite)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            if (!await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                return Forbid();
            }

            Site site = await _utile.SiteDeKey(keySite);
            if (site == null)
            {
                return NotFound();
            }

            AFacturer àFacturer = await _service.AFacturer(site);

            return Ok(àFacturer);
        }

        /// <summary>
        /// retourne la liste des CommandeAFacturer avec Détails d'un client
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        [HttpGet("/api/facture/enCours")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Commandes([FromQuery] KeyUidRno keyClient)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                return Forbid();
            }

            List<CommandeAFacturer> commandes = await _service.CommandesAFacturer(site, keyClient);

            return Ok(commandes);
        }

        #endregion

        #region Détail

        [HttpPut("/api/facture/detail")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Detail(DétailCommandeVue vue)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyUidRno keySite = new KeyUidRno { Uid = vue.Uid2, Rno = vue.Rno };

            if (!await carte.EstActifEtAMêmeUidRno(keySite.KeyParam))
            {
                return Forbid();
            }

            Commande commande = await _service.CommandeDeDétail(vue);
            if (commande == null)
            {
                return NotFound();
            }

            if (!(await _service.EstDansSynthèseEnvoyée(commande)))
            {
                Erreurs.ErreurDeModel.AjouteAModelState(ModelState, "CommandeLivréeEtNonFacturée");
                return BadRequest();
            }

            DétailCommande détail = await _service.LitDétail(vue);
            if (détail == null)
            {
                return NotFound();
            }

            RetourDeService retour = await _service.EcritDétail(détail, vue);

            return SaveChangesActionResult(retour);
        }

        #endregion

        #region Commande

        public async Task<IActionResult> ActionCommande([FromQuery] KeyUidRnoNo keyCommande, Func<Commande, Task<RetourDeService>> action)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyUidRno keyClient = new KeyUidRno { Uid = keyCommande.Uid, Rno = keyCommande.Rno };
            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                return Forbid();
            }

            Commande commande = await _service.LitCommande(keyCommande);
            if (commande == null)
            {
                return NotFound();
            }

            if (!(await _service.EstDansSynthèseEnvoyée(commande)))
            {
                Erreurs.ErreurDeModel.AjouteAModelState(ModelState, "CommandeLivréeEtNonFacturée");
            }

            RetourDeService retour = await action(commande);

            return SaveChangesActionResult(retour);
        }

        [HttpPost("/api/facture/copieCommande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieCommande([FromQuery] KeyUidRnoNo keyCommande)
        {
            Task<RetourDeService> action(Commande commande) => _service.CopieCommande(commande);
            return await ActionCommande(keyCommande, action);
        }

        [HttpPost("/api/facture/annuleCommande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleCommande([FromQuery] KeyUidRnoNo keyCommande)
        {
            Task<RetourDeService> action(Commande commande) => _service.AnnuleCommande(commande);
            return await ActionCommande(keyCommande, action);
        }

        #endregion

        #region Commandes

        public async Task<IActionResult> ActionCommandes([FromQuery] KeyUidRno keyClient, Func<Site, KeyUidRno, List<Commande>, Task<RetourDeService>> action)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                return Forbid();
            }

            List<Commande> commandes = await _service.CommandesLivréesEtNonFacturées(site, keyClient);
            if (commandes.Count == 0)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(ModelState, "PasDeCommandeLivréeEtNonFacturée");
                return BadRequest();
            }

            RetourDeService retour = await action(site, keyClient, commandes);

            return SaveChangesActionResult(retour);
        }

        [HttpPost("/api/facture/copieCommandes")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> CopieCommandes([FromQuery] KeyUidRno keyClient)
        {
            Task<RetourDeService> action(Site site, KeyUidRno keyClient1,List<Commande> commandes) => _service.CopieCommandes(commandes);
            return await ActionCommandes(keyClient, action);
        }

        [HttpPost("/api/facture/annuleCommandes")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> AnnuleCommandes([FromQuery] KeyUidRno keyClient)
        {
            Task<RetourDeService> action(Site site, KeyUidRno keyClient1,List<Commande> commandes) => _service.AnnuleCommandes(commandes);
            return await ActionCommandes(keyClient, action);
        }

        [HttpPost("/api/facture/facture")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(409)] // Conflict
        public async Task<IActionResult> Facture([FromQuery] KeyUidRno keyClient)
        {
            Task<RetourDeService> action(Site site, KeyUidRno keyClient1, List<Commande> commandes) => _service.FactureCommandes(site, keyClient, commandes);
            return await ActionCommandes(keyClient, action);
        }

        #endregion
    }
}
