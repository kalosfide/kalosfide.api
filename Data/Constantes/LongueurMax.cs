using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class LongueurMax
    {
        public const int Id = 450; // lu dans les colonnes de la table ApplicationUser
        public const int Email = 256; // lu dans les colonnes de la table ApplicationUser
        public const int UId = 20; // UInt64.MaxValue.ToString().Length;
        public const int RoleNo = 10; // UInt32.MaxValue.ToString().Length;
    }
}
