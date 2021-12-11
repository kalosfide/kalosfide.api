using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Roles
{
    public interface IRoleService : Partages.KeyParams.IKeyUidRnoService<Role, RoleVue>
    {
        DateTime ChangeEtatSansSauver(Role role, string état);
        Task<RetourDeService<RoleEtat>> ChangeEtat(Role role, string état);
        bool TempsInactifEcoulé(DateTime date);

        /// <summary>
        /// retourne le site d'un objet ayant un UidRno qui est celui du role qui en est propriétaire
        /// </summary>
        /// <param name="akeyRole">Role ou Client ou Fournisseur</param>
        /// <returns></returns>
        Task<Site> SiteDeRole(AKeyBase akeyRole);
    }
}
