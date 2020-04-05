using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Factures
{
    public interface IFactureService : IBaseService
    {

        /// <summary>
        /// Retourne les ClientAFacturer de tous les clients ayant des commandes livrées non facturées
        /// contenant les CommandeAFacturer sans Détails
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<AFacturer> AFacturer(Site site);

        /// <summary>
        /// retourne la liste des CommandeAFacturer avec Détails d'un client
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <returns></returns>
        Task<List<CommandeAFacturer>> CommandesAFacturer(Site site, AKeyUidRno keyClient);

        Task<Commande> CommandeDeDétail(AKeyUidRnoNo2 aKeyDétail);

        Task<bool> EstDansSynthèseEnvoyée(Commande commande);

        Task<DétailCommande> LitDétail(DétailCommandeVue vue);

        Task<RetourDeService> EcritDétail(DétailCommande détail, DétailCommandeVue vue);

        Task<Commande> LitCommande(AKeyUidRnoNo keyCommande);
        Task<RetourDeService> CopieCommande(Commande commande);
        Task<RetourDeService> AnnuleCommande(Commande commande);

        Task<List<Commande>> CommandesLivréesEtNonFacturées(Site site, AKeyUidRno keyClient);
        Task<RetourDeService> CopieCommandes(List<Commande> commande);
        Task<RetourDeService> AnnuleCommandes(List<Commande> commande);

        Task<RetourDeService> FactureCommandes(Site site, AKeyUidRno keyClient, List<Commande> commandes);
    }
}
