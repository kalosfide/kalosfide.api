using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class NomsRéservés
    {
        static string NomAnonymeBase = "X";
        static string NomAnonymeSéparateur = "_";

        public static string AdresseAnonyme = "x rue X";

        /// <summary>
        /// retourne le nom anonyme construit avec le numéro d'anonymat
        /// </summary>
        /// <param name="noAnonymat"></param>
        /// <returns></returns>
        public static string NomAnonyme(int noAnonymat)
        {
            return NomAnonymeBase + NomAnonymeSéparateur + noAnonymat.ToString();
        }

        /// <summary>
        /// retourne le numéro d'anonymat contenu dans le nom si c'est un nom anonyme, 0 sinon
        /// </summary>
        /// <param name="nom">nom</param>
        /// <returns></returns>
        public static int NoAnonyme(string nom)
        {
            try
            {
                string[] split = nom.Split(NomAnonymeSéparateur);
                if (split.Count() == 2 && split[0] == NomAnonymeBase)
                {
                    return int.Parse(split[1]);
                
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }
        public static bool EstComme(string nom, string baseNom)
        {
            int nbNom = nom.Count();
            int nbBase = baseNom.Count();
            if (nbNom <= nbBase)
            {
                return nom == baseNom.Substring(0, nbNom);
            }
            return baseNom == nom.Substring(0, nbBase);
        }
        public static bool EstCommeAnonyme(string nom)
        {
            return EstComme(nom, NomAnonymeBase);
        }
        public static bool EstRéservé(string nom)
        {
            return EstComme(nom, NomAnonymeBase);
        }
    }
}
