using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeClf
    {
        public const string Commande = "C";
        public const string Livraison = "L";
        public const string Facture = "F";

        public static string TypeBon(string typeSynthèse)
        {
            return typeSynthèse == Livraison ? Commande : typeSynthèse == Facture ? Livraison : null;
        }

        public static string TypeSynthèse(string typeBon)
        {
            return typeBon == Commande ? Livraison : typeBon == Livraison ? Facture : null;
        }
    }
}
