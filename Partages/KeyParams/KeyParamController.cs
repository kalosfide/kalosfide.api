using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public abstract class KeyParamController<T, TVue> : AvecCarteController where T : AKeyBase where TVue : AKeyBase
    {
        protected IKeyParamService<T, TVue> __service;

        public KeyParamController(IKeyParamService<T, TVue> service, IUtilisateurService utilisateurService) : base(utilisateurService)
        {
            __service = service;
        }

        /// <summary>
        /// Fixe le numéro (Rno ou No suivant la surcharge) au premier disponible
        /// </summary>
        /// <param name="vue">vue à ajouter</param>
        /// <returns></returns>
        protected abstract Task FixeKeyParamAjout(TVue vue);

        /// <summary>
        /// ajoute la donnée définie par la vue
        /// </summary>
        /// <param name="carte">Carte utilisateur comportant une erreur si l'ajout n'est pas autorisé</param>
        /// <param name="vue">contient tous les champs pour créer une donnée</param>
        /// <returns></returns>
        protected async Task<IActionResult> Ajoute(CarteUtilisateur carte, TVue vue)
        {
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            await FixeKeyParamAjout(vue);
            T donnée = __service.CréeDonnée(vue);

            DValideModel<T> dValideAjoute = __service.DValideAjoute();
            if (dValideAjoute != null)
            {
                await dValideAjoute(donnée, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }

            RetourDeService<T> retour = await __service.Ajoute(donnée);

            if (retour.Ok)
            {
                return StatusCode(201, vue.KeyParam);
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Modifie une donnée par les champs présents dans la vue
        /// </summary>
        /// <param name="carte">Carte utilisateur comportant une erreur si l'édition n'est pas autorisée</param>
        /// <param name="donnée">donnée à modifier si elle existe</param>
        /// <param name="vue">contient les champs à modifier dans la donnée</param>
        /// <returns></returns>
        protected async Task<IActionResult> Edite(CarteUtilisateur carte, T donnée, TVue vue)
        {
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            // vérifie l'existence de la donnée
            if (donnée == null)
            {
                return NotFound();
            }

            // vérifie que les valeurs à changer sont valides
            DValideModel<T> dValide = __service.DValideEdite();
            if (dValide != null)
            {
                T àValider = __service.CréeDonnéeEditéeComplète(vue, donnée);
                await dValide(àValider, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }

            RetourDeService<T> retour = await __service.Edite(donnée, vue);

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Supprime une donnée.
        /// </summary>
        /// <param name="carte">Carte utilisateur comportant une erreur si l'édition n'est pas autorisée</param>
        /// <param name="donnée">donnée à supprimer si elle existe</param>
        /// <returns></returns>
        protected async Task<IActionResult> Supprime(CarteUtilisateur carte, T donnée)
        {
            if (carte.Erreur != null)
            {
                return carte.Erreur;
            }

            if (donnée == null)
            {
                return NotFound();
            }

            DValideModel<T> dValideSupprime = __service.DValideSupprime();
            if (dValideSupprime != null)
            {
                await dValideSupprime(donnée, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }

            var retour = await __service.Supprime(donnée);

            return SaveChangesActionResult(retour);
        }

    }
}