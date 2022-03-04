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
    /// <typeparam name="TAjouté">Objet avec Id à retourner après un ajout à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public interface IAvecIdUintService<T, TAjout, TAjouté, TEdite> where T: AvecIdUint where TAjouté : AvecIdUint where TEdite: AvecIdUint
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

        T CréeDonnéeEditéeComplète(TEdite vue, T donnéePourCompléter);

        Task<T> Lit(uint id);

        Task<RetourDeService<TAjouté>> Ajoute(TAjout ajout, ModelStateDictionary modelState);
        Task<RetourDeService<TAjouté>> Ajoute(TAjout ajout, ModelStateDictionary modelState, DateTime date);

        Task<RetourDeService<TAjouté>> AjouteSansValider(T donnée);
        Task<RetourDeService<TAjouté>> AjouteSansValider(T donnée, DateTime date);

        Task<RetourDeService<T>> Edite(T donnée, TEdite nouveau);
        void SupprimeSansSauver(T donnée);
        Task<RetourDeService<T>> Supprime(T donnée);
    }
}
