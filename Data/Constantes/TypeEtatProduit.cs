using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeEtatProduit
    {
        public const string Disponible = "D";
        public const string Indisponible = "I";
        public const string Supprimé = "S";

        public static bool EstValide(string état)
        {
            return (new string[]
            {
                Disponible,
                Indisponible,
                Supprimé
            }).Contains(état);
        }
    }
}
