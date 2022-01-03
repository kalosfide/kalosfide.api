using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{

    /// <summary>
    /// ajoute au ModelState les éventuelles erreurs de la donnée
    /// </summary>
    /// <param name="donnée"></param>
    /// <param name="modelState"></param>
    /// <returns></returns>
    public delegate Task DAvecIdUintValideModel<T>(T donnée, ModelStateDictionary modelState) where T : AvecIdUint;

    /// <summary>
    /// Service CRUD de base.
    /// </summary>
    /// <typeparam name="T">Entité de la base de donnée</typeparam>
    /// <typeparam name="TAjout">Objet sans Id pour ajouter à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public interface IAvecIdUintService<T, TAjout, TEdite> where T: AvecIdUint where TEdite: AvecIdUint
    {
        /// <summary>
        /// validateur qui ajoute au ModelState les éventuelles erreurs de la donnée
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DAvecIdUintValideModel<T> DValideAjoute();
        /// <summary>
        /// validateur qui ajoute au ModelState les éventuelles erreurs de la donnée
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DAvecIdUintValideModel<T> DValideEdite();
        /// <summary>
        /// validateur qui ajoute une erreur au ModelState si la donnée n'est pas supprimable
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DAvecIdUintValideModel<T> DValideSupprime();

        Task<T> CréeDonnée(TAjout vue);
        T CréeDonnéeEditéeComplète(TEdite vue, T donnéePourCompléter);

        Task<T> Lit(uint id);

        Task<RetourDeService<T>> Ajoute(TAjout ajout, ModelStateDictionary modelState);

        Task<RetourDeService<T>> Ajoute(T donnée);
        Task<RetourDeService<T>> Ajoute(T donnée, DateTime date);
        void EditeSansSauver(T donnée, TEdite vue);
        Task<RetourDeService<T>> Edite(T donnée, TEdite nouveau);
        void SupprimeSansSauver(T donnée);
        Task<RetourDeService<T>> Supprime(T donnée);
    }
}
