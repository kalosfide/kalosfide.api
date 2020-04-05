using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Commandes;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Data.Constantes;
using System;
using KalosfideAPI.Utiles;

namespace KalosfideAPI.Livraisons
{
    public class LivraisonService : BaseService, ILivraisonService
    {
        private readonly IUtileService _utile;
        private readonly ICommandeService _commandeService;

        public LivraisonService(ApplicationContext context,
            IUtileService utile, ICommandeService commandeService
            ) : base(context)
        {
            _utile = utile;
            _commandeService = commandeService;
        }

        #region Utiles

        /// <summary>
        /// retourne la livraison définie par la clé
        /// </summary>
        /// <param name="keyLivraison"></param>
        /// <returns>null si non trouvé</returns>
        private async Task<Livraison> LivraisonDeKey(KeyUidRnoNo keyLivraison)
        {
            Livraison livraison = await _context.Livraison
                .Where(l => l.Uid == keyLivraison.Uid && l.Rno == keyLivraison.Rno)
                .FirstOrDefaultAsync();
            return livraison;
        }

        #endregion // Utiles

        /// <summary>
        /// crée une livraison et fixe le numéro de livraison des commandes sans numéro de livraison
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task CommenceLivraison(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            long no = livraison == null ? 1 : livraison.No + 1;
            livraison = new Livraison
            {
                Uid = site.Uid,
                Rno = site.Rno,
                No = no
            };
            _context.Livraison.Add(livraison);

            /// fixe le numéro de livraison des commandes sans numéro de livraison émises par !es clients autorisés d'un site
            await _commandeService.CommenceLivraison(site, no);
        }

        /// <summary>
        /// retourne la dernière livraison du site si elle n'est pas terminée
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Livraison> LivraisonPasDeLivraisonOuTerminée(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison.Date.HasValue)
            {
                // la dernière livraison est terminée
                return null;
            }
            return livraison;
        }

        /// <summary>
        /// retourne la dernière livraison du site si elle n'est pas terminée
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Livraison> LivraisonPasTerminée(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison.Date.HasValue)
            {
                // la dernière livraison est terminée
                return null;
            }
            return livraison;
        }

        /// <summary>
        /// retourne la dernière livraison du site si elle n'est pas terminée
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Livraison> LivraisonATerminer(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            if (livraison.Date.HasValue)
            {
                // la dernière livraison est terminée
                return null;
            }
            return livraison;
        }

        /// <summary>
        /// retourne vrai si tous les ALivrer des détails des commandes de la livraison sont fixés
        /// </summary>
        /// <param name="livraison"></param>
        /// <returns></returns>
        public async Task<bool> EstPrête(Livraison livraison)
        {
            bool pasPrête = await _context.Commande
                .Where(c => c.SiteUid == livraison.Uid && c.SiteRno == livraison.Rno && c.LivraisonNo == livraison.No)
                .Where(c => c.Détails.Where(d => !d.ALivrer.HasValue).Any())
                .AnyAsync();
            return !pasPrête;
        }

        /// <summary>
        /// supprime une livraison et supprime le numéro de livraison des commandes qui y étaient affectées
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task AnnuleLivraison(Site site, Livraison livraison)
        {
            _context.Livraison.Remove(livraison);

            /// supprime le numéro de livraison des commandes de la livraison
            await _commandeService.AnnuleLivraison(site, livraison.No);
        }

        /// <summary>
        /// fixe la date de la livraison, supprime les commandes vides de la livraison et fixe la date des autres
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task TermineLivraison(Site site, Livraison livraison)
        {
            livraison.Date = DateTime.Now;
            _context.Livraison.Update(livraison);

            // supprime les commandes vides, fixe la date des autres à celle de leur dernier détail créé par le client s'il y en a, à celle de la livraison sinon.
            await _commandeService.TermineLivraison(site, livraison);
        }

        /// <summary>
        /// retourne un objet LivraisonVue contenant les dernières commandes non vides des clients
        /// </summary>
        /// <param name="site">un site</param>
        /// <returns></returns>
        public async Task<LivraisonVue> LivraisonVueEnCours(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            long no = livraison == null ? 1 : site.Etat == TypeEtatSite.Livraison ? livraison.No : livraison.No + 1;
            DateTime? date = livraison == null ? null : livraison.Date;
            List<Commande> dernièresCommandes = await _commandeService.DernièresCommandes(site);
            LivraisonVue vue = new LivraisonVue
            {
                Uid = site.Uid,
                Rno = site.Rno,
                No = no,
                DateLivraison = date,
                Commandes = dernièresCommandes.Select(c => _commandeService.CréeCommandeVue(c)).ToList(),
                Date = DateTime.Now
            };
            return vue;
        }

        /// <summary>
        /// retourne un objet LivraisonVue contenant les commandes reçues depuis la date
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<LivraisonVue> VueDesCommandesOuvertesDesClientsAvecCompte(Site site)
        {
            Livraison livraison = await _utile.DernièreLivraison(site);
            long no = livraison == null ? 1 : site.Etat == TypeEtatSite.Livraison ? livraison.No : livraison.No + 1;
            List<Commande> dernièresCommandes = await _commandeService.CommandesOuvertesDesClientsAvecCompte(site);
            LivraisonVue vue = new LivraisonVue
            {
                Uid = site.Uid,
                Rno = site.Rno,
                No = no,
                Commandes = dernièresCommandes.Select(c => _commandeService.CréeCommandeVue(c)).ToList(),
                Date = DateTime.Now
            };
            return vue;
        }

    }
}
