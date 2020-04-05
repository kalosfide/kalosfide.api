using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using System;

namespace KalosfideAPI.Démarrage
{
    public class DBInitializeConfig
    {
        private readonly IUtilisateurService _service;

        public DBInitializeConfig(IUtilisateurService service)
        {
            _service = service;
        }

        public void DataTest()
        {
            Users();
        }

        private void Users()
        {
            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@kalosfide.fr",
            };
//            var retour = _service.Enregistre(admin, password);
        }

    }
}
