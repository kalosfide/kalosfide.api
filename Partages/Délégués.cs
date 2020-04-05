using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{

    /// <summary>
    /// ajoute au ModelState les éventuelles erreurs de la donnée
    /// </summary>
    /// <param name="donnée"></param>
    /// <param name="modelState"></param>
    /// <returns></returns>
    public delegate Task DValideModel<T>(T donnée, ModelStateDictionary modelState) where T : AKeyBase;

    public delegate TData DCréeData<T, TData>(T donnée);
    public delegate Task<TData> DCréeDataAsync<T, TData>(T donnée);

    /// <summary>
    /// retourne false si l'utilisateur à une permission de lecture ou d'écriture sur la donnée
    /// </summary>
    /// <param name="carte"></param>
    /// <param name="donnée"></param>
    /// <returns></returns>
    public delegate Task<bool> DInterdictionCarte<T>(CarteUtilisateur carte, T donnée);

    /// <summary>
    /// retourne true si la donnée est verrouillée pour une action de lecture ou d'écriture
    /// </summary>
    /// <param name="donnée"></param>
    /// <returns></returns>
    public delegate Task<bool> DInterdiction<T>(T donnée);

    public delegate Task<RetourDeService<T>> DEdite<T, TVue>(T donnée, TVue vue) where T: class;

}
