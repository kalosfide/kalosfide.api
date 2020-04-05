using System;
using System.Linq;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeEtatCommande
    {
        public const string Nouveau = "N";
        public const string Envoyé = "E";
        public const string APréparer = "A";
        public const string ARefuser = "B";
        public const string Préparé = "P";
        public const string Refusé = "R";
        public const string Facturé = "F";
        public static bool EstValide(string etat)
        {
            return (new string[]
            {
                Nouveau,
                Envoyé,
                APréparer,
                ARefuser,
                Préparé,
                Refusé,
                Facturé
            }).Contains(etat);
        }
    }
}
