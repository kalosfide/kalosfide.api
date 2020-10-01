using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public class ConfirmeChangeEmailVue
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
