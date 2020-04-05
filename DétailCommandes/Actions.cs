using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.DétailCommandes
{

    public delegate bool DVérifieEtatCommande(Commande commande);
    public delegate void DVérifieDétail(AKeyUidRnoNo2 aKeyDétail, Produit produit, ModelStateDictionary modelState);
    public delegate Task<DétailCommande> DDétailDeVue(AKeyUidRnoNo2 aKeyDétail);
    public delegate Task<RetourDeService> DAction(AKeyUidRnoNo2 aKeyDétail, bool parLeClient);
    public delegate Task<RetourDeService> DActionAvecDétail(AKeyUidRnoNo2 aKeyDétail, DétailCommande détailCommande, bool parLeClient);
    public delegate Task<RetourDeService> DActionAvecCommandeEtDétail(AKeyUidRnoNo2 aKeyDétail, Commande commande, DétailCommande détailCommande, bool parLeClient);
    public delegate Task<RetourDeService> DActionSite(AKeyUidRno aKeySite);

    public class ActionDétailDef
    {
        public DVérifieEtatCommande VérifieEtatCommande { get; set; }

        /// <summary>
        /// vérifie la conformité au modèle
        /// </summary>
        public DVérifieDétail Vérifie { get; set; }
        public DDétailDeVue DétailDeVue { get; set; }
        public DAction Action { get; set; }
        public DActionAvecDétail ActionAvecDétail { get; set; }
        public DActionAvecCommandeEtDétail ActionAvecCommandeEtDétail { get; set; }
    }
}
