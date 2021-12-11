using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{
    public interface IAdminService
    {
        Task<List<Fournisseur>> Fournisseurs();
        Task<Fournisseur> Fournisseur(KeyUidRno keyRole);
    }
}
