using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Roles;
using KalosfideAPI.Utilisateurs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Administrateurs
{
    public class AdministrateurService : RoleService, IAdministrateurService
    {
        private readonly IUtilisateurService _utilisateurService;

        public AdministrateurService(
            ApplicationContext context,
            IUtilisateurService utilisateurService
        ) : base(context)
        {
            _utilisateurService = utilisateurService;
        }
    }
}
