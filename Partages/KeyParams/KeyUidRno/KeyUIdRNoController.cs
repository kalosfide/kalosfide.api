using KalosfideAPI.Data.Keys;
using KalosfideAPI.Utilisateurs;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    public abstract class KeyUidRnoController<T, TVue> : KeyParamController<T, TVue> where T : AKeyUidRno where TVue : AKeyUidRno
    {

        public KeyUidRnoController(IKeyUidRnoService<T, TVue> service, IUtilisateurService utilisateurService) : base(service, utilisateurService)
        {
        }

        private IKeyUidRnoService<T, TVue> _service { get => __service as IKeyUidRnoService<T, TVue>; }

        protected async override Task FixeKeyParamAjout(TVue vue)
        {
            vue.Rno = await _service.DernierNo(vue.Uid) + 1;
        }
    }
}