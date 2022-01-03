using KalosfideAPI.Data;
using KalosfideAPI.Utiles;
using KalosfideAPI.Utilisateurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public class DevenirClientVue: IClientData, ICréeCompteVue
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Code { get; set; }
    }
}
