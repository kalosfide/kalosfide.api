using KalosfideAPI.Data;
using KalosfideAPI.Erreurs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    public abstract class BaseController : Controller
    {

        /// <summary>
        /// modèle des erreurs BadRequest
        /// </summary>
        /// <param name="erreur"></param>
        /// <returns></returns>
        protected IActionResult RésultatBadRequest(string code, string champ)
        {
            ErreurDeModel.AjouteAModelState(ModelState, code, champ);
            return BadRequest(ModelState);
        }

        /// <summary>
        /// modèle des erreurs BadRequest
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected IActionResult RésultatBadRequest(string code)
        {
            ErreurDeModel.AjouteAModelState(ModelState, code);
            return BadRequest(ModelState);
        }

        public IActionResult SaveChangesActionResult(RetourDeService retour)
        {
            switch (retour.Type)
            {
                case TypeRetourDeService.Ok:
                    return NoContent();
                case TypeRetourDeService.IdentityError:
                    return StatusCode(500, "La création du compte utilisateur a échoué.");
                case TypeRetourDeService.ConcurrencyError:
                    return StatusCode(409);
                case TypeRetourDeService.UpdateError:
                    return RésultatBadRequest(retour.Message);
                case TypeRetourDeService.Indéterminé:
                    return StatusCode(500, retour.Message);
                default:
                    break;
            }
            return StatusCode(500, "Erreur interne inconnue.");
        }
    }
}
