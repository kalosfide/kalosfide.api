using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public interface IKeyParamService<T, TVue, TParam> : IBaseService<T> where T: AKeyBase where TVue: AKeyBase where TParam: KeyParam
    {
        /// <summary>
        /// validateur qui ajoute au ModelState les éventuelles erreurs de la donnée
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DValideModel<T> DValideAjoute();
        /// <summary>
        /// validateur qui ajoute au ModelState les éventuelles erreurs de la donnée
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DValideModel<T> DValideEdite();
        /// <summary>
        /// validateur qui ajoute une erreur au ModelState la donnée n'est pas supprimable
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        DValideModel<T> DValideSupprime();

        TVue CréeVue(T donnée);
        T CréeDonnée(TVue vue);
        T CréeDonnéeEditéeComplète(TVue vue, T donnéePourComplèter);
        void CopieVueDansDonnée(T donnée, TVue vue);

        void AjouteSansSauver(T donnée);
        Task<RetourDeService<T>> Ajoute(T donnée);
        void EditeSansSauver(T donnée, TVue vue);
        Task<RetourDeService<T>> Edite(T donnée, TVue nouveau);
        Task SupprimeSansSauver(T donnée);
        Task<RetourDeService<T>> Supprime(T donnée);

        Task<T> Lit(TParam param);
        Task<TVue> LitVue(TParam param);
        Task<List<TVue>> ListeVue(TParam param);
    }
}
