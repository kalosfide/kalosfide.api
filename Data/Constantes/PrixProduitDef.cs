using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public class PrixProduitDef: DécimalDef
    {
        public const int Précision = 7;
        public const int Décimales = 2;
        public const string Type = "decimal(7,2)";

        /// <summary>
        /// Vérifie le nombre de chiffres avant et après la virgule de l'écriture décimale du prix
        /// </summary>
        /// <param name="précisionDef"></param>
        /// <param name="décimalesDef"></param>
        /// <param name="prix"></param>
        /// <returns>'chiffres' ou 'decimales' s'il y a une erreur, null s'il n'y a pas d'erreur</returns>
        public static string Vérifie(decimal prix)
        {
            return DécimalDef.Vérifie(Précision, Décimales, prix);
        }

        public static string MessageVérifie(string nom, decimal quantité)
        {
            return MessageVérifie(Précision, Décimales, nom, quantité);
        }

        public const decimal Max = 9 * 10 ^ (Précision + Décimales) / 10 ^ Décimales;

    }
}
