using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public class DécimalDef
    {
        /// <summary>
        /// Vérifie le nombre de chiffres avant et après la virgule de l'écriture décimale de la quantité
        /// </summary>
        /// <param name="précisionDef"></param>
        /// <param name="décimalesDef"></param>
        /// <param name="quantité"></param>
        /// <returns>'chiffres' ou 'decimales' s'il y a une erreur, null s'il n'y a pas d'erreur</returns>
        protected static string Vérifie(int précisionDef, int décimalesDef, decimal quantité)
        {
            string[] textes = quantité.ToString().Split('.');
            int ordreDeGrandeur = textes[0].Length;
            int décimales = textes.Length > 1 ? textes[1].Length : 0;
            if (ordreDeGrandeur + décimales > précisionDef)
            {
                return "chiffres.";
            }
            if (décimales > décimalesDef)
            {
                return "decimales";
            }
            return null;
        }
        protected static string MessageVérifie(int précisionDef, int décimalesDef, string nom, decimal quantité)
        {
            if (quantité <= 0)
            {
                return nom + " doit être positif ou nul.";
            }
            string[] textes = quantité.ToString().Split('.');
            int ordreDeGrandeur = textes[0].Length;
            int décimales = textes.Length > 1 ? textes[1].Length : 0;
            if (ordreDeGrandeur + décimales > précisionDef)
            {
                return nom + " ne peut pas avoir plus de " + précisionDef + " chiffres.";
            }
            if (décimales > décimalesDef)
            {
                return nom + " ne peut pas avoir plus de " + décimalesDef + " chiffres après la virgule.";
            }
            return null;
        }

    }
    public class QuantitéDef: DécimalDef
    {
        public const int Précision = 8;
        public const int Décimales = 3;
        public const string Type = "decimal(8,3)";

        /// <summary>
        /// retourne "chiffres" ou "decimales" si la quantité ne satisfait pas au format décimal de la BDD
        /// </summary>
        /// <param name="quantité"></param>
        /// <returns></returns>
        public static string Vérifie(decimal quantité)
        {
            return Vérifie(Précision, Décimales, quantité);
        }

        /// <summary>
        /// retourne un message d'erreur si la quantité ne satisfait pas au format décimal de la BDD
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="quantité"></param>
        /// <returns></returns>
        public static string MessageVérifie(string nom, decimal quantité)
        {
            return MessageVérifie(Précision, Décimales, nom, quantité);
        }

        public const decimal Max = 9 * 10 ^ (Précision + Décimales) / 10 ^ Décimales;
    }
}
