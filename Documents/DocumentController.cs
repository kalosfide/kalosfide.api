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

namespace KalosfideAPI.Documents
{

    [ApiController]
    [Route("UidRnoNo")]
    [Authorize]
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _service;
        private readonly IUtileService _utile;
        private readonly IUtilisateurService _utilisateurService;
        private readonly IClientService _clientService;

        public DocumentController(IDocumentService service,
            IUtileService utile,
            IUtilisateurService utilisateurService,
            IClientService clientService)
        {
            _service = service;
            _utile = utile;
            _utilisateurService = utilisateurService;
            _clientService = clientService;
        }

        /// <summary>
        /// Retourne les liste des commandes livrées non facturées regroupées par client et la liste des No et Date des livraisons de ces commandes
        /// </summary>
        /// <param name="keySite">key du fournisseur</param>
        /// <returns></returns>
        [HttpGet("/api/document/listeC")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> ListeC([FromQuery] KeyUidRno keyClient)
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

            if (!carte.EstClient(site))
            {
                return Forbid();
            }

            Documents documents = await _service.ListeC(site, keyClient);

            return Ok(documents);
        }

        /// <summary>
        /// Retourne les liste des commandes livrées non facturées regroupées par client et la liste des No et Date des livraisons de ces commandes
        /// </summary>
        /// <param name="keySite">key du fournisseur</param>
        /// <returns></returns>
        [HttpGet("/api/document/listeF")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> ListeF([FromQuery] KeyUidRno keySite)
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

            Documents documents = await _service.ListeF(site);

            return Ok(documents);
        }

        private async Task<IActionResult> Document(KeyUidRnoNo keyDocument, Func<Site, KeyUidRnoNo, Task<AKeyUidRnoNo>> litDocument)
        {
            CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
            if (carte == null)
            {
                // fausse carte
                return Forbid();
            }

            KeyUidRno keyClient = new KeyUidRno
            {
                Uid = keyDocument.Uid,
                Rno = keyDocument.Rno
            };

            Site site = await _utile.SiteDeClient(keyClient);
            if (site == null)
            {
                return NotFound();
            }

            if (!await carte.EstActifEtAMêmeUidRno(site.KeyParam))
            {
                // l'utilisateur n'est pas le fournisseur
                if (!carte.EstClient(site))
                {
                    return Forbid();
                }
            }

            AKeyUidRnoNo document = await litDocument(site, keyDocument);
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);

        }

        /// <summary>
        /// Retourne le bon de commande
        /// </summary>
        /// <param name="keyDocument">key de la commande</param>
        /// <returns></returns>
        [HttpGet("/api/document/commande")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Commande([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, (Site site, KeyUidRnoNo key) => _service.Commande(site, key));
        }

        /// <summary>
        /// Retourne le bon de livraison
        /// </summary>
        /// <param name="keyDocument">key de la livraison</param>
        /// <returns></returns>
        [HttpGet("/api/document/livraison")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Livraison([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, (Site site, KeyUidRnoNo key) => _service.Livraison(site, key));
        }

        /// <summary>
        /// Retourne la facture
        /// </summary>
        /// <param name="keyDocument">key de la facture</param>
        /// <returns></returns>
        [HttpGet("/api/document/facture")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(403)] // Forbid
        [ProducesResponseType(404)] // Not found
        public async Task<IActionResult> Facture([FromQuery] KeyUidRnoNo keyDocument)
        {
            return await Document(keyDocument, (Site site, KeyUidRnoNo key) => _service.Facture(site, key));
        }

    }
}
