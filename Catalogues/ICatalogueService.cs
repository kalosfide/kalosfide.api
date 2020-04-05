using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public interface ICatalogueService
    {
        /// <summary>
        /// retourne la dernière date de fin de modification du catalogue antérieure à la date si présente ou null si la modification est en cours
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<DateTime> DateCatalogue(AKeyUidRno keySite, DateTime? date);


        /// <summary>
        /// archive les changements du catalogue depuis le début de la modification
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task ArchiveModifications(Site site);

        /// <summary>
        /// Crée un tarif (catalogue avec données datées) à partir des archives passées en paramètre.
        /// </summary>
        /// <param name="aKeySite">un Site ou son UidRno</param>
        /// <param name="archivesProduit"></param>
        /// <param name="archivesCatégorie"></param>
        /// <param name="anciensSeulement">si vrai seules les archives antérieures à la date du catalogue complet sont utilisées</param>
        /// <returns></returns>
        Catalogue Tarif(AKeyUidRno aKeySite, IEnumerable<ArchiveProduit> archivesProduit, IEnumerable<ArchiveCatégorie> archivesCatégorie, bool anciensSeulement);

        /// <summary>
        /// retourne le catalogue complet du site actuellement en vigueur
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<Catalogue> Complet(KeyUidRno keySite);


        /// <summary>
        /// retourne le catalogue des disponibilités du site actuellement en vigueur
        /// </summary>
        /// <param name="keySite"></param>
        /// <returns></returns>
        Task<Catalogue> Disponibles(KeyUidRno keySite);

        /// <summary>
        /// Retourne le catalogue complet si il n'y a pas de date ou si la date est antérieure à la dernière modification du catalogue,
        /// un catalogue sans produits sinon.
        /// Le catalogue retourné a une date égale à DateNulle si une modification du catalogue est en cours.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<Catalogue> Obsolète(Site site, DateTime? date);

    }
}
