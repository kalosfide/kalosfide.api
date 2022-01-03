using KalosfideAPI.Data;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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

        protected IActionResult RésultatCréé(object objet)
        {
            return StatusCode(201, objet);
        }

        /// <summary>
        /// Fixe les valeurs de propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// Si la valeur après traitement d'une propriété est null ou vide, une ErreurDeModel avec le nom de cette propriétés est ajoutée au ModelState.
        /// </summary>
        /// <param name="objet">Object ayant les propriétés à traiter. Si null, une exception est levée.</param>
        /// <param name="àVérifier">Array d'objets contenant les noms des propriétés à traiter et les actions à effectuer si la valeur
        /// de la propriété est nulle ou vide.
        /// Si pour l'un des noms l'objet n'a pas de propriété de ce nom ou si la propriété n'est pas de type string, une exception est levée.</param>
        public void VérifieSansEspaces(object objet, SansEspacesPropertyDef[] àVérifier)
        {
            SansEspaces sansEspaces = SansEspaces.AuDébutNiALaFinNiSuccessifs;
            sansEspaces.FixeValeur(objet, àVérifier);
        }

        /// <summary>
        /// Fixe les valeurs de propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// Si la valeur après traitement d'une propriété est null ou vide, une ErreurDeModel avec le nom de cette propriétés est ajoutée au ModelState.
        /// </summary>
        /// <param name="objet">Object ayant les propriétés à traiter. Si null, une exception est levée.</param>
        /// <param name="àVérifier">Array d'objets contenant les noms des propriétés à traiter et les actions à effectuer si la valeur
        /// de la propriété est nulle ou vide.
        /// Si pour l'un des noms l'objet n'a pas de propriété de ce nom ou si la propriété n'est pas de type string, une exception est levée.</param>
        private void VérifieSansEspaces(object objet, string[] nomsPropriétés, Func<string, SansEspacesPropertyDef> créeDef)
        {
            SansEspacesPropertyDef[] àVérifier = new SansEspacesPropertyDef[nomsPropriétés.Length];
            for (int i = 0; i < nomsPropriétés.Length; i++)
            {
                àVérifier[i] = créeDef(nomsPropriétés[i]);
            }
            SansEspaces sansEspaces = SansEspaces.AuDébutNiALaFinNiSuccessifs;
            sansEspaces.FixeValeur(objet, àVérifier);
        }

        /// <summary>
        /// Fixe les valeurs de propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// Si la valeur après traitement d'une propriété est null ou vide, une ErreurDeModel avec le nom de cette propriétés est ajoutée au ModelState.
        /// </summary>
        /// <param name="objet">Object ayant les propriétés à traiter. Si null, une exception est levée.</param>
        /// <param name="àVérifier">Array d'objets contenant les noms des propriétés à traiter et les actions à effectuer si la valeur
        /// de la propriété est nulle ou vide.
        /// Si pour l'un des noms l'objet n'a pas de propriété de ce nom ou si la propriété n'est pas de type string, une exception est levée.</param>
        public void VérifieSansEspacesData(object objet, string[] nomsPropriétés)
        {
            Func<string, SansEspacesPropertyDef> créeDef = (string nomPropriété) => new SansEspacesPropertyDef(nomPropriété)
            {
                QuandNull = QuandNull,
                QuandVide = QuandStringVide
            };
            VérifieSansEspaces(objet,nomsPropriétés, créeDef);
        }

        /// <summary>
        /// Fixe les valeurs de propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// Si la valeur après traitement d'une propriété est null ou vide, une ErreurDeModel avec le nom de cette propriétés est ajoutée au ModelState.
        /// </summary>
        /// <param name="objet">Object ayant les propriétés à traiter. Si null, une exception est levée.</param>
        /// <param name="àVérifier">Array d'objets contenant les noms des propriétés à traiter et les actions à effectuer si la valeur
        /// de la propriété est nulle ou vide.
        /// Si pour l'un des noms l'objet n'a pas de propriété de ce nom ou si la propriété n'est pas de type string, une exception est levée.</param>
        public void VérifieSansEspacesDataAnnulable(object objet, string[] nomsPropriétés)
        {
            SansEspacesPropertyDef créeDef(string nomPropriété) => new SansEspacesPropertyDef(nomPropriété)
            {
                QuandVide = QuandStringVide
            };
            VérifieSansEspaces(objet,nomsPropriétés, créeDef);
        }

        /// <summary>
        /// Marque dans le ModelState une ErreurDeModel Null pour la propriété.
        /// </summary>
        /// <param name="nomPropriété">nom de la propriété qui a une valeur nulle alors qu'il ne le faut pas.</param>
        private void QuandNull(string nomPropriété)
        {
            ErreurDeModel.AjouteAModelState(ModelState, nomPropriété, "Null");
        }

        /// <summary>
        /// Marque dans le ModelState une ErreurDeModel Vide pour la propriété de type string.
        /// </summary>
        /// <param name="nomPropriété">nom de la propriété de type string qui a une valeur vide alors qu'il ne le faut pas.</param>
        private void QuandStringVide(string nomPropriété)
        {
            ErreurDeModel.AjouteAModelState(ModelState, nomPropriété, "Vide");
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
