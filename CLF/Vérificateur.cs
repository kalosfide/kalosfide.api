using KalosfideAPI.Data.Keys;
using KalosfideAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Clients;

namespace KalosfideAPI.CLF
{
    public delegate Task DVérifieInformation(Vérificateur vérificateur);

    public class VérificationException : Exception
    {
        public VérificationException()
        { }
    }

    public class Vérificateur
    {
        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public AKeyUidRnoNo KeyDoc { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public AKeyUidRnoNo2 KeyLigne { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public CLFLigne CLFLigne { get; set; }

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
        /// commande concernée par l'action si elle existe dans la BDD
        /// </summary>
        public DocCLF DocCLF { get; set; }

        /// <summary>
        /// détail concerné par l'action s'il existe dans la BDD
        /// </summary>
        public LigneCLF LigneCLF { get; set; }

        /// <summary>
        /// Dernière archive du produit demandé dans la ligne
        /// </summary>
        public ArchiveProduit ArchiveProduit { get; set; }

        /// <summary>
        /// No de la livraison cible
        /// </summary>
        public long NoLivraison { get; set; }

        /// <summary>
        /// Date du catalogue
        /// </summary>
        public DateTime? DateCatalogue { get; set; }

        /// <summary>
        /// présent si la vérification ne peut être complètée
        /// </summary>
        public IActionResult Erreur { get; set; }

        public List<Func<Vérificateur, Task>> Vérifications = new List<Func<Vérificateur, Task>>();

        public Vérificateur()
        {
        }
        public void Initialise(ParamsKeyClient paramsCrée)
        {
            KeyClient = paramsCrée;
            DateCatalogue = paramsCrée.DateCatalogue;
        }
        public void Initialise(AKeyUidRno keyClient)
        {
            KeyClient = keyClient;
        }
        public void Initialise(AKeyUidRnoNo keyDoc)
        {
            KeyClient = AKeyUidRnoNo.KeyUidRno(keyDoc);
            KeyDoc = keyDoc;
        }
        public void Initialise(ParamsKeyDoc paramsSupprime)
        {
            KeyClient = AKeyUidRnoNo.KeyUidRno(paramsSupprime);
            KeyDoc = paramsSupprime;
            DateCatalogue = paramsSupprime.DateCatalogue;
        }
        public void Initialise(AKeyUidRnoNo2 keyLigne)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(keyLigne);
            KeyDoc = AKeyUidRnoNo2.KeyUidRnoNo_1(keyLigne);
            KeyLigne = keyLigne;
        }
        public void Initialise(ParamsKeyLigne paramsKeyLigne)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(paramsKeyLigne);
            KeyDoc = AKeyUidRnoNo2.KeyUidRnoNo_1(paramsKeyLigne);
            KeyLigne = paramsKeyLigne;
            DateCatalogue = paramsKeyLigne.DateCatalogue;
        }
        public void Initialise(CLFLigne clfLigne)
        {
            KeyClient = AKeyUidRnoNo2.KeyUidRno_1(clfLigne);
            KeyDoc = AKeyUidRnoNo2.KeyUidRnoNo_1(clfLigne);
            KeyLigne = new KeyUidRnoNo2();
            KeyLigne.CopieKey(clfLigne.KeyParam);
            CLFLigne = clfLigne;
        }
        public void Initialise(CLFLigne clfLigne, ParamsVide paramsLigne)
        {
            Initialise(clfLigne);
            if (paramsLigne != null)
            {
                DateCatalogue = paramsLigne.DateCatalogue;
            }
        }

        public void Ajoute(params Func<Vérificateur, Task>[] vérifications)
        {
            Vérifications.AddRange(vérifications);
        }

        public async Task Vérifie()
        {
            for (int i = 0; i < Vérifications.Count; i++)
            {
                await Vérifications[i](this);
                if (Erreur != null)
                {
                    return;
                }
            }
        }
    }
}
