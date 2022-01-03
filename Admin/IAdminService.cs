using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{
    public interface IAdminService
    {
        Task<List<FournisseurVue>> Fournisseurs();
        Task<FournisseurVue> Fournisseur(uint idFournisseur);
    }
}
