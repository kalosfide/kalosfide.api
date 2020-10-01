using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyUidRnoNoService<T, TVue> : IKeyParamService<T, TVue, KeyParam> where T : AKeyUidRnoNo where TVue: AKeyUidRnoNo
    {
        Task<long> DernierNo(KeyParam param);

        Task<Site> SiteDeDonnée(T donnée);

        /// <summary>
        /// remplace les états archivés des données modifiées depuis la date passée en paramètres par leur valeur finale
        /// </summary>
        /// <param name="depuis"></param>
        /// <returns></returns>
        Task RésumeArchives(AKeyUidRno keySite, DateTime jusquA, DateTime? depuis);

        /// <summary>
        /// retourne la date de la dernière archive vérifiant le filtre et enregistrée avant la date de fin
        /// </summary>
        /// <param name="filtre"></param>
        /// <param name="jusquA">DateTime de fin</param>
        /// <returns></returns>
        Task<DateTime?> DateArchive(AKeyUidRno keySite, DateTime? jusquA);
    }
}
