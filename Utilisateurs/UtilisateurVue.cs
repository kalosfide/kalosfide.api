using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Utilisateurs
{
    public class UtilisateurVue
    {
        public string UserId { get; set; }
        public string UtilisateurId { get; set; }

        public string Nom { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public long RoleSélectionnéId { get; set; }
        public Role RoleSélectionné { get; set; }

        public ICollection<Role> Roles { get; set; }
    }
}
