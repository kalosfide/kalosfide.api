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

}
