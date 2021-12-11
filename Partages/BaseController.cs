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
        /// <param name="code">nom du validateur</param>
        /// <param name="champ">nom du champ du formulaire</param>
        /// <returns></returns>
        protected IActionResult RésultatBadRequest(string champ, string code)
        {
            ErreurDeModel.AjouteAModelState(ModelState, champ, code);
            return BadRequest(ModelState);
        }

        /// <summary>
        /// modèle des erreurs BadRequest
        /// </summary>
        /// <param name="code">nom du validateur du formulaire</param>
        /// <returns></returns>
        protected IActionResult RésultatBadRequest(string code)
        {
            ErreurDeModel.AjouteAModelState(ModelState, code);
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Modèle des Erreurs Forbid
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected IActionResult RésultatInterdit(string message)
        {
            return StatusCode(403, new { Message = message });
        }

        public IActionResult SaveChangesActionResult(RetourDeService retour)
        {
            switch (retour.Type)
            {
                case TypeRetourDeService.Ok:
                    return Ok();
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
