using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public class PeupleClientVue: ICréeCompteVue, IClientData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
    }
}
