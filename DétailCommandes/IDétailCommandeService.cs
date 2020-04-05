using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.DétailCommandes
{
    public interface IDétailCommandeService : IKeyParamService<DétailCommande, DétailCommandeVue, KeyParam>
    {

        #region Utiles

        /// <summary>
        /// retourne le détail référencé, les champs Commande, Commande.Livraison et Livraison.Facture sont inclus
        /// </summary>
        /// <param name="aKeyDétail">KeyUidRnoNo2 d'un détail ou DétailCommande ou DétailCommandeVue</param>
        /// <returns></returns>
        Task<DétailCommande> Détail(AKeyUidRnoNo2 aKeyDétail);

        /// <summary>
        /// retourne la Commande du détail référencé
        /// </summary>
        /// <param name="aKeyDétail">KeyUidRnoNo2 d'un détail ou DétailCommande ou DétailCommandeVue</param>
        /// <returns></returns>
        Task<Commande> Commande(AKeyUidRnoNo2 aKeyDétail);

        decimal CoûtDemande(IEnumerable<DétailCommandeData> détails, Catalogue catalogue, out bool incomplet);

        decimal CoûtALivrer(IEnumerable<DétailCommandeData> détails, Catalogue catalogue);

        decimal CoûtAFacturer(IEnumerable<DétailCommandeData> détails, Catalogue catalogue);

        DétailCommandeData DétailCommandeData(DétailCommande détailCommande);

        #endregion


        Task<RetourDeService<DétailCommande>> Ajoute(DétailCommandeVue vue);

        /// <summary>
        /// crée des copie des détails dont le produit est toujours disponible et les ajoute sans sauver à la BDD
        /// </summary>
        /// <param name="commande"></param>
        Task<RetourDeService> AjouteCopiesDétails(Commande commande);

    }
}
