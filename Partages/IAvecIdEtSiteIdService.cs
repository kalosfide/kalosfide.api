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
    /// Extension du service CRUD de base.
    /// </summary>
    /// <typeparam name="T">Entité de la base de donnée</typeparam>
    /// <typeparam name="TAjout">Objet sans Id pour ajouter à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public interface IAvecIdEtSiteIdService<T, TAjout, TEdite> : IAvecIdUintService<T, TAjout, TEdite>
         where T : AvecIdUint, IAvecSiteId where TEdite : AvecIdUint
    {
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
        Task<bool> TermineModification(uint idSite, DateTime dateDébut, DateTime dateFin);
    }
}
