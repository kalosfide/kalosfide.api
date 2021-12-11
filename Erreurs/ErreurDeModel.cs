using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Erreurs
{
    public static class ErreurDeModel
    {

        /// <summary>
        /// code lorsque l'erreur concerne plusieurs champs
        /// </summary>
        const string Champs = "2";

        /// <summary>
        /// ajoute une erreur d'un champ au ModelStateDictionary d'un controller
        /// </summary>
        /// <param name="modelState">ModelStateDictionary d'un controller</param>
        /// <param name="champ">nom du champ générant l'erreur</param>
        /// <param name="code">nom du validateur du champ concerné par l'erreur ou message</param>
        public static void AjouteAModelState(ModelStateDictionary modelState, string champ, string code)
        {
            modelState.TryAddModelError(champ, code);
        }

        /// <summary>
        /// ajoute une erreur concernant plusieurs champs au ModelStateDictionary d'un controller
        /// </summary>
        /// <param name="modelState">ModelStateDictionary d'un controller</param>
        /// <param name="code">nom de l'erreur</param>
        /// <param name="champ">nom du champ générant l'erreur ou code Champs</param>
        public static void AjouteAModelState(ModelStateDictionary modelState, string code)
        {
            modelState.TryAddModelError(Champs, code);
        }
    }
}
