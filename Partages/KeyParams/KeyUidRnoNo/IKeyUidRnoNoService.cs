using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyUidRnoNoService<T, TVue> : IKeyParamService<T, TVue> where T : AKeyUidRnoNo, IAvecDate where TVue: AKeyUidRnoNo
    {
        Task<long> DernierNo(AKeyUidRnoNo key);

        Task<T> Lit(AKeyUidRnoNo key);

        Task<Site> SiteDeDonnée(T donnée);

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les archives créées enregistrées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="site">le site</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        Task<bool> TermineModification(Site site, DateTime dateFin);
    }
}
