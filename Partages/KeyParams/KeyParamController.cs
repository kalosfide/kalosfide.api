using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public abstract class KeyParamController<T, TVue, TParam> : BaseController where T : AKeyBase where TVue : AKeyBase where TParam : KeyParam
    {
        protected IKeyParamService<T, TVue, TParam> __service;

        protected IUtilisateurService _utilisateurService;

        /// <summary>
        /// si présent et vrai pour une donnée, interdit les actions d'écriture sur la donnée
        /// </summary>
        protected DInterdiction<T> dEcritVerrouillé;

        /// <summary>
        /// si présent et vrai pour une carte d'utilisateur et une donnée, interdit d'ajouter la donnée
        /// </summary>
        protected DInterdictionCarte<KeyParam> dAjouteInterdit;
        protected DInterdictionCarte<KeyParam> dEditeInterdit;
        protected DInterdictionCarte<KeyParam> dSupprimeInterdit;
        protected DInterdictionCarte<KeyParam> dLitInterdit;
        protected DInterdictionCarte<KeyParam> dListeInterdit;

        public KeyParamController(IKeyParamService<T, TVue, TParam> service, IUtilisateurService utilisateurService)
        {
            __service = service;
            _utilisateurService = utilisateurService;
        }

        protected abstract void FixePermissions();

        /// <summary>
        /// par défaut toutes les actions sont interdites
        /// </summary>
        /// <param name="carte"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        protected Task<bool> Interdiction(CarteUtilisateur carte, KeyParam param)
        {
            return Task.FromResult(true);
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
        /// <param name="vue">contient tous les champs pour créer une donnée</param>
        /// <returns></returns>
        public async Task<IActionResult> Ajoute(TVue vue)
        {
            if (dAjouteInterdit != null)
            {
                CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
                if (carte == null)
                {
                    // fausse carte
                    return Forbid();
                }

                if (await dAjouteInterdit(carte, vue.KeyParamParent))
                {
                    return Forbid();
                }
            }

            await FixeKeyParamAjout(vue);
            T donnée = __service.CréeDonnée(vue);

            if (dEcritVerrouillé != null && await dEcritVerrouillé(donnée))
            {
                return Conflict();
            }

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
                return CreatedAtAction(nameof(Lit), vue);
            }

            return SaveChangesActionResult(retour);
        }

        /// <summary>
        /// remplace la donnée ayant la même clé que la vue par la donnée définie par la vue
        /// </summary>
        /// <param name="vue">contient tous les champs pour créer une donnée</param>
        /// <returns></returns>
        public async Task<IActionResult> Edite(TVue vue)
        {
            // vérifie les droits de l'utilisateur
            if (dEditeInterdit != null)
            {
                CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
                if (carte == null)
                {
                    // fausse carte
                    return Forbid();
                }

                if (await dEditeInterdit(carte, vue.KeyParam))
                {
                    return Forbid();
                }
            }

            // vérifie l'existence de la donnée
            T donnée = await __service.Lit(vue.KeyParam as TParam);
            if (donnée == null)
            {
                return NotFound();
            }

            // vérifie que l'écriture est possible
            if (dEcritVerrouillé != null && await dEcritVerrouillé(donnée))
            {
                return Conflict();
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

        public async Task<IActionResult> Supprime(TParam param)
        {
            if (dSupprimeInterdit != null)
            {
                CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
                if (carte == null)
                {
                    // fausse carte
                    return Forbid();
                }

                if (await dSupprimeInterdit(carte, param))
                {
                    return Forbid();
                }
            }

            var donnée = await __service.Lit(param);
            if (donnée == null)
            {
                return NotFound();
            }

            if (dEcritVerrouillé != null && await dEcritVerrouillé(donnée))
            {
                return Conflict();
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

        protected async Task<IActionResult> Lit(Func<TParam, Task<object>> litObjet, TParam param)
        {
            if (dLitInterdit != null)
            {
                CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
                if (carte == null)
                {
                    // fausse carte
                    return Forbid();
                }

                if (await dLitInterdit(carte, param))
                {
                    return Forbid();
                }
            }

            object objet = await litObjet(param);
            if (objet == null)
            {
                return NotFound();
            }

            return Ok(objet);
        }
        public async Task<IActionResult> Lit(TParam param)
        {
            return await Lit(async (p) => await __service.LitVue(p), param);
        }

        protected async Task<IActionResult> Liste(Func<Task<object>> litListe, KeyParam param)
        {
            if (dListeInterdit != null)
            {
                CarteUtilisateur carte = await _utilisateurService.CréeCarteUtilisateur(HttpContext.User);
                if (carte == null)
                {
                    // fausse carte
                    return Forbid();
                }

                if (await dListeInterdit(carte, param))
                {
                    return Forbid();
                }
            }

           object objet = await litListe();
            if (objet == null)
            {
                return NotFound();
            }

            return Ok(objet);
        }
        public async Task<IActionResult> Liste(KeyParam param)
        {
            return await Liste(async () => await __service.ListeVue(param), param);
        }
        public async Task<IActionResult> Liste()
        {
            return await Liste(async () => await __service.Liste(), null);
        }
        protected async Task<IActionResult> Liste(KeyParam param, DFiltre<TVue> valide)
        {
            return await Liste(async () => await __service.Liste(param, valide), param);
        }
        protected async Task<IActionResult> Liste(DFiltre<TVue> valide)
        {
            return await Liste(async () => await __service.Liste(valide), null);
        }

    }
}