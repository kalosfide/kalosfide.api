using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    /// <summary>
    /// outil du KeyParamService de T, Tvue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEdite"></typeparam>
    public interface IAvecIdEtSiteIdGèreArchive<T, TEdite>: IAvecIdUintGèreArchive<T, TEdite> where T : AvecIdUint, IAvecSiteId where TEdite : AvecIdUint
    {
        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="idSite">id d'un site</param>
        /// <param name="dateDébut">date à laquelle la modification des données a commencé</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>la liste des données modifiées si des modifications ont eu lieu, null sinon.</returns>
        Task<List<T>> TermineModification(uint idSite, DateTime dateDébut, DateTime dateFin);
    }

    /// <summary>
    /// outil du KeyParamService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEdite"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class AvecIdEtSiteIdGèreArchive<T, TEdite, TArchive> : AvecIdUintGèreArchive<T, TEdite, TArchive>, IAvecIdEtSiteIdGèreArchive<T, TEdite>
        where T : AvecIdUint, IAvecSiteId where TEdite : AvecIdUint where TArchive : AvecIdUint, IAvecDate
    {
        /// <summary>
        /// Gestionnaire des archives d'un type de données.
        /// </summary>
        /// <param name="dbSetArchive">DbSet des archives</param>
        public AvecIdEtSiteIdGèreArchive(DbSet<TArchive> dbSetArchive): base(dbSetArchive)
        {
        }

        /// <summary>
        /// Archives incluant leur donnée.
        /// </summary>
        /// <returns></returns>
        protected abstract IQueryable<TArchive> ArchivesAvecDonnée(uint idSite);

        protected abstract T DonnéeDeArchive(TArchive archive);

        protected abstract void CopieArchiveDansArchive(TArchive de, TArchive vers);

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="idSite">Id d'un site</param>
        /// <param name="dateDébut">date à laquelle la modification des données a commencé</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>la liste des données modifiées si des modifications ont eu lieu, null sinon.</returns>
        public async Task<List<T>> TermineModification(uint idSite, DateTime dateDébut, DateTime dateFin)
        {
            List<TArchive> résumés = new List<TArchive>();
            List<T> donnéesModifiées = new List<T>();

                // recherche les archives enregistrées depuis le début de la modification
            List<TArchive> nouvellesArchives = await ArchivesAvecDonnée(idSite).ToListAsync();
            if (nouvellesArchives.Count() == 0)
            {
                return null;
            }
            List<List<TArchive>> archivesParDonnée = nouvellesArchives
                // groupe par donnée
                .GroupBy(a => a.Id)
                .Select(g => g.ToList())
                .ToList();

            archivesParDonnée.ForEach(archives =>
            {
                T donnée = DonnéeDeArchive(archives.First());
                donnée.Date = dateFin;
                donnéesModifiées.Add(donnée);
                TArchive résumé;
                TArchive seed = CréeArchive();
                seed.Id = donnée.Id;
                seed.Date = dateFin;
                résumé = archives.OrderBy(a => a.Date).Aggregate(
                    seed,
                    (r, a) =>
                    {
                        CopieArchiveDansArchive(a, r);
                        return r;
                    });
                résumés.Add(résumé);
            });
            _dbSetArchive.RemoveRange(nouvellesArchives);
            _dbSetArchive.AddRange(résumés);
            return donnéesModifiées;
        }

    }

    /// <summary>
    /// Service CRUD de base.
    /// </summary>
    /// <typeparam name="T">Entité de la base de donnée</typeparam>
    /// <typeparam name="TAjout">Objet sans Id pour ajouter à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public abstract class AvecIdEtSiteIdService<T, TAjout, TEdite> : AvecIdUintService<T, TAjout, TEdite>, IAvecIdEtSiteIdService<T, TAjout, TEdite>
         where T : AvecIdUint, IAvecSiteId where TEdite : AvecIdUint
    {
        protected AvecIdEtSiteIdService(ApplicationContext context) : base(context)
        {
        }

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Remplace les archives concernent la même donnée par une seule archive de date la date de fin résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="idSite">Id d'un site</param>
        /// <param name="dateDébut">date à laquelle la modification des données a commencé</param>
        /// <param name="dateFin">date à laquelle la modification des données a fini</param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        public async Task<bool> TermineModification(uint idSite, DateTime dateDébut, DateTime dateFin)
        {
            List<T> donnéesModifiées = await ((IAvecIdEtSiteIdGèreArchive<T, TEdite>)_gèreArchive).TermineModification(idSite, dateDébut, dateFin);
            if (donnéesModifiées == null)
            {
                return false;
            }
            _dbSet.UpdateRange(donnéesModifiées);
            return true;
        }
    }
}
