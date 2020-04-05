using KalosfideAPI.Data.Keys;
using KalosfideAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Clients;

namespace KalosfideAPI.Commandes
{
    public delegate Task DVérifieInformation(Vérificateur vérificateur);

    public class Vérificateur
    {
        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public AKeyUidRnoNo KeyCommande { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public AKeyUidRnoNo2 KeyDétail { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public DétailCommandes.DétailCommandeVue VueDétail { get; set; }

        /// <summary>
        /// clé du client propriétaire de la commande
        /// </summary>
        public AKeyUidRno KeyClient { get; set; }

        /// <summary>
        /// vue contenant les donnéees d'état du client
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// site du client
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        /// vrai si l'utilisateur est le client
        /// </summary>
        public bool EstClient { get; set; }

        /// <summary>
        /// vrai si l'utilisateur est le fournisseur du site
        /// </summary>
        public bool EstFournisseur { get; set; }

        /// <summary>
        /// dernière commande du client si elle existe
        /// </summary>
        public Commande DernièreCommande { get; set; }

        /// <summary>
        /// commande concernée par l'action si elle existe dans la BDD
        /// </summary>
        public Commande Commande{ get; set; }

        /// <summary>
        /// détail concerné par l'action s'il existe dans la BDD
        /// </summary>
        public DétailCommande Détail { get; set; }

        /// <summary>
        /// produit demandé dans le détail
        /// </summary>
        public Produit Produit { get; set; }

        /// <summary>
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime DateCatalogue { get; set; }

        /// <summary>
        /// présent si la vérification ne peut être complètée
        /// </summary>
        public IActionResult Erreur { get; set; }

        public Vérificateur()
        {
        }
        public Vérificateur(ParamsCréeCommande paramsCrée)
        {
            KeyClient = paramsCrée;
            NoLivraison = paramsCrée.NoLivraison;
            DateCatalogue = paramsCrée.DateCatalogue;
        }
        public Vérificateur(ParamsSupprimeCommande paramsSupprime)
        {
            KeyClient = AKeyUidRnoNo.KeyUidRno(paramsSupprime);
            KeyCommande = paramsSupprime;
            NoLivraison = paramsSupprime.NoLivraison;
            DateCatalogue = paramsSupprime.DateCatalogue;
        }
        public Vérificateur(AKeyUidRnoNo2 keyDétail)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(keyDétail);
            KeyCommande = AKeyUidRnoNo2.KeyUidRnoNo_1(keyDétail);
            KeyDétail = keyDétail;
        }
        public Vérificateur(ParamsSupprimeDétail paramsDétail)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(paramsDétail);
            KeyCommande = AKeyUidRnoNo2.KeyUidRnoNo_1(paramsDétail);
            KeyDétail = paramsDétail;
            NoLivraison = paramsDétail.NoLivraison;
            DateCatalogue = paramsDétail.DateCatalogue;
        }
        public Vérificateur(DétailCommandes.DétailCommandeVue vueDétail, ParamsEditeDétail paramsDétail)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(vueDétail);
            KeyCommande = AKeyUidRnoNo2.KeyUidRnoNo_1(vueDétail);
            KeyDétail = new KeyUidRnoNo2();
            KeyDétail.CopieKey(vueDétail.KeyParam);
            VueDétail = vueDétail;
            if (paramsDétail != null)
            {
                NoLivraison = paramsDétail.NoLivraison;
                DateCatalogue = paramsDétail.DateCatalogue;
            }
        }

        public async Task Vérifie(params Func<Vérificateur, Task>[] vérifications)
        {
            for (int i = 0; i < vérifications.Length; i++)
            {
                await vérifications[i](this);
                if (Erreur != null)
                {
                    return;
                }
            }
        }
    }
}
