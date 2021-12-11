using System;
using System.Linq;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeEtatSite
    {
        public const string Ouvert = "O";
        public const string Catalogue = "C";
        public const string Banni = "X";
        public static bool EstValide(string etat)
        {
            return (new string[]
            {
                Ouvert,
                Catalogue,
                Banni
            }).Contains(etat);
        }
    }
}
