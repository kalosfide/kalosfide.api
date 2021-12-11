using KalosfideAPI.Data.Keys;
using KalosfideAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Clients;
using KalosfideAPI.Data.Constantes;

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
        public Role Client { get; set; }

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
        /// Date du catalogue contenu dans le paramètre si l'utilisateur est client
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

        /// <summary>
        /// Fixe KeyClient et DateCatalogue.
        /// </summary>
        /// <param name="paramsCrée">contient la KeyUidRno du client et la date du catalogue de l'utilisateur</param>
        public void Initialise(ParamsKeyClient paramsCrée)
        {
            KeyClient = paramsCrée;
            DateCatalogue = paramsCrée.DateCatalogue;
        }

        /// <summary>
        /// Fixe KeyClient.
        /// </summary>
        /// <param name="aKeyClient">objet ayant la même KeyUidRno que le client</param>
        public void Initialise(AKeyUidRno aKeyClient)
        {
            KeyClient = aKeyClient;
        }

        /// <summary>
        /// Fixe KeyClient et KeyDoc.
        /// </summary>
        /// <param name="keyDocSansType">objet ayant la même KeyUidRnoNo que le document</param>
        public void Initialise(KeyUidRnoNo keyDocSansType)
        {
            KeyClient = AKeyUidRnoNo.KeyUidRno(keyDocSansType);
            KeyDoc = keyDocSansType;
        }

        /// <summary>
        /// Fixe KeyClient, KeyDoc et DateCatalogue.
        /// </summary>
        /// <param name="paramsSupprime">contient la KeyUidRnoNo du document et la date du catalogue de l'utilisateur</param>
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
            KeyLigne.CopieKey(clfLigne);
            CLFLigne = clfLigne;
        }
        public void Initialise(CLFLigne clfLigne, ParamsVide paramsLigne)
        {
            Initialise(clfLigne);
            if (paramsLigne != null && paramsLigne.DateCatalogue.HasValue)
            {
                DateCatalogue = paramsLigne.DateCatalogue;
            }
        }
    }
}
