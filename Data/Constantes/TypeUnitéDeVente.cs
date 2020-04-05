using KalosfideAPI.Erreurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class UnitéDeMesure
    {
        public const string Aucune = "U";
        public const string Kilo = "K";
        public const string Litre = "L";

        public static bool EstValide(string type)
        {
            return type == Aucune || type == Kilo || type == Litre;
        }

        public static string UnitéDeCommandeParDéfaut(string typeMesure)
        {
            if (typeMesure == Aucune)
            {
                return TypeUnitéDeCommande.Unité;
            }
            else
            {
                return TypeUnitéDeCommande.Vrac;
            }
        }

    }
    public static class TypeUnitéDeCommande
    {
        public const string Unité = "1";
        public const string Vrac = "2";
        public const string UnitéOuVrac = "3";

        public static bool EstValide(string type, string typeMesure)
        {
            return type == Unité || ((type == Vrac || type == UnitéOuVrac) && typeMesure != UnitéDeMesure.Aucune);
        }

        public static bool DemandeEstValide(string typeDemande, string typeProduit)
        {
            return (typeProduit == Unité && (typeDemande == Unité))
                || (typeProduit == Vrac && (typeDemande == Vrac))
                || (typeProduit == UnitéOuVrac && (typeDemande == Unité || typeDemande == Vrac));
        }

        public static string TypeCopiable(string typeProduit)
        {
            return typeProduit == Unité
                ? Unité
                : typeProduit == Vrac || typeProduit == UnitéOuVrac
                    ? Vrac
                    : null;
        }
    }
}
