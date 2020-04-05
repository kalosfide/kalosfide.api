using System;
using System.Linq;

namespace KalosfideAPI.Data.Constantes
{
    public class TypeEtatUtilisateur
    {
        public const string Nouveau = "N";
        public const string Actif = "A";
        public const string Inactif = "I";
        public const string Banni = "X";
        public static bool EstValide(string etat)
        {
            return (new string[]
            {
                Nouveau,
                Actif,
                Inactif,
                Banni
            }).Contains(etat);
        }
    }
}
