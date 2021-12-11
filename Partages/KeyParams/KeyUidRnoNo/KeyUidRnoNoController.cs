using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public abstract class KeyUidRnoNoController<T, TVue> : KeyParamController<T, TVue> where T : AKeyUidRnoNo, IAvecDate where TVue : AKeyUidRnoNo
    {

        public KeyUidRnoNoController(IKeyUidRnoNoService<T,TVue> service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
        }

        private IKeyUidRnoNoService<T,TVue> _service { get => __service as IKeyUidRnoNoService<T, TVue>; }

        /// <summary>
        /// Crée la carte utilisateur définie par le HttpContext.
        /// Vérifie que l'utilisateur existe et est actif et que la session est la même qu'à la connection.
        /// Vérifie que le role correspondant au site existe et est actif.
        /// Fixe le Role de la carte.
        /// Vérifie que le role est celui du fournisseur du site.
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <param name="keySite">objet ayant les Uid et Rno du site</param>
        /// <returns>la carte a l'erreur Forbid si l'utilisateur n'est pas fournisseur actif du site, Conflict si le site n'est pas d'état Catalogue,
        /// Unauthorized si la session est périmée</returns>
        protected async Task<CarteUtilisateur> CréeCarteFournisseurCatalogue(IKeyUidRno keySite)
        {
            CarteUtilisateur carte = await CréeCarteFournisseur(keySite);
            if (carte.Erreur == null)
            {
                Site site = carte.Role.Site;
                if (site.Ouvert)
                {
                    carte.Erreur = Conflict();
                }
            }
            return carte;
        }

        protected async override Task FixeKeyParamAjout(TVue vue)
        {
            vue.No = await _service.DernierNo(vue) + 1;
        }

    }
}
