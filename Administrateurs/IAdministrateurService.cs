using KalosfideAPI.Data;
using KalosfideAPI.Partages.KeyParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Administrateurs
{
    public interface IAdministrateurService: IKeyUidRnoService<Administrateur, AdministrateurVue>
    {
    }
}
