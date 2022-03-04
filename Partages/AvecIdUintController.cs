using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    /// <summary>
    /// Service CRUD de base.
    /// </summary>
    /// <typeparam name="T">Entité de la base de donnée</typeparam>
    /// <typeparam name="TAjout">Objet sans Id pour ajouter à la base de donnée</typeparam>
    /// <typeparam name="TAjouté">Objet avec Id à retourner après un ajout à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public abstract class AvecIdUintController<T, TAjout, TAjouté, TEdite> : AvecCarteController
        where T : AvecIdUint where TAjouté : AvecIdUint where TEdite : AvecIdUint
    {
        protected IAvecIdUintService<T, TAjout, TAjouté, TEdite> __service;

        public AvecIdUintController(IAvecIdUintService<T, TAjout, TAjouté,  TEdite> service, IUtilisateurService utilisateurService) : base(utilisateurService)
        {
            __service = service;
        }

        /// <summary>
        /// ajoute la donnée définie par la vue
        /// </summary>
        /// <param name="ajout">contient tous les champs pour créer une donnée</param>
        /// <returns></returns>
        protected async Task<IActionResult> Ajoute(TAjout ajout)
        {
            RetourDeService<TAjouté> retour = await __service.Ajoute(ajout, ModelState);
            if (retour.ModelError)
            {
                return BadRequest(ModelState);
            }

            if (retour.Ok)
            {
                return RésultatCréé(retour.Entité);
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// Modifie une donnée par les champs présents dans la vue
        /// </summary>
        /// <param name="donnée">donnée à modifier</param>
        /// <param name="vue">contient les champs à modifier dans la donnée</param>
        /// <returns></returns>
        protected async Task<IActionResult> Edite(T donnée, TEdite vue)
        {
            // vérifie que les valeurs à changer sont valides
            DAvecIdUintValideModel<T> dValide = __service.DValideEdite();
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

            DAvecIdUintValideModel<T> dValideSupprime = __service.DValideSupprime();
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