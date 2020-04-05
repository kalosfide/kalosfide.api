using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Enregistrement
{
    public class AdministrateurVue: VueBase
    {
        public Administrateur CréeAdministrateur()
        {
            return new Administrateur
            {
            };
        }
    }
}
