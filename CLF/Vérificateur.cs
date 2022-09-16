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
        public IKeyDocSansType KeyDoc { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public IKeyLigneSansType KeyLigne { get; set; }

        /// <summary>
        /// paramètre de la requête POST ou PUT ou DELETE
        /// </summary>
        public CLFLigne CLFLigne { get; set; }

        /// <summary>
        /// clé du client propriétaire de la commande
        /// </summary>
        public uint IdClient { get; set; }

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
        /// Fournisseur du site, null si l'utilisateur n'est pas le Fournisseur
        /// </summary>
        public Fournisseur Fournisseur { get; set; }

        /// <summary>
        /// commande concernée par l'action si elle existe dans la BDD
        /// </summary>
        public DocCLF DocCLF { get; set; }

        /// <summary>
        /// détail concerné par l'action s'il existe dans la BDD
        /// </summary>
        public LigneCLF LigneCLF { get; set; }

        /// <summary>
        /// Produit demandé dans la ligne de commande.
        /// </summary>
        public Produit Produit { get; set; }

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
            IdClient = paramsCrée.Id;
            DateCatalogue = paramsCrée.DateCatalogue;
        }

        /// <summary>
        /// Fixe IdClient.
        /// </summary>
        /// <param name="idClient">objet ayant la même KeyUidRno que le client</param>
        public void Initialise(uint idClient)
        {
            IdClient = idClient;
        }

        /// <summary>
        /// Fixe KeyClient et KeyDoc.
        /// </summary>
        /// <param name="keyDocSansType">objet ayant la même KeyUidRnoNo que le document</param>
        public void Initialise(KeyDocSansType keyDocSansType)
        {
            IdClient = keyDocSansType.Id;
            KeyDoc = keyDocSansType;
        }

        /// <summary>
        /// Fixe KeyClient, KeyDoc et DateCatalogue.
        /// </summary>
        /// <param name="paramsSupprime">contient la KeyUidRnoNo du document et la date du catalogue de l'utilisateur</param>
        public void Initialise(ParamsKeyDoc paramsSupprime)
        {
            IdClient = paramsSupprime.Id;
            KeyDoc = paramsSupprime;
            DateCatalogue = paramsSupprime.DateCatalogue;
        }
        public void Initialise(KeyLigneSansType keyLigne)
        {
            IdClient = keyLigne.Id;
            KeyDoc = KeyLigneSansType.KeyDocSansType(keyLigne);
            KeyLigne = keyLigne;
        }
        public void Initialise(ParamsKeyLigne paramsKeyLigne)
        {
            IdClient = paramsKeyLigne.Id;
            KeyDoc = KeyLigneSansType.KeyDocSansType(paramsKeyLigne);
            KeyLigne = paramsKeyLigne;
            DateCatalogue = paramsKeyLigne.DateCatalogue;
        }
        public void Initialise(CLFLigne clfLigne)
        {
            IdClient = clfLigne.Id;
            KeyDoc = clfLigne;
            KeyLigne = clfLigne;
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
