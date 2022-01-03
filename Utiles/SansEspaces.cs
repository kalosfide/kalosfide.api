using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{

    [Serializable]
    public class SansEspacesException : Exception
    {
        public SansEspacesException() { }
        public SansEspacesException(string message) : base(message) { }
        public SansEspacesException(string message, Exception inner) : base(message, inner) { }
        protected SansEspacesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class SansEspacesPropertyDef
    {
        public string Nom { get; set; }
        public Action<string> QuandNull { get; set; }
        public Action<string> QuandVide { get; set; }

        public SansEspacesPropertyDef(string nom)
        {
            Nom = nom;
        }
    }

    public class SansEspaces
    {
        protected internal bool Début { get; set; }
        protected internal bool Fin { get; set; }
        protected internal bool Successif { get; set; }

        private SansEspaces()
        {
        }

        /// <summary>
        /// Retourne un objet SansEspaces qui enlève les char WhiteSpace au début d'une string.
        /// </summary>
        public static SansEspaces AuDébut { get { return new SansEspaces { Début = true, Fin = false, Successif = false }; } }
        /// <summary>
        /// Retourne un objet SansEspaces qui enlève les char WhiteSpace à la fin d'une string.
        /// </summary>
        public static SansEspaces ALaFin { get { return new SansEspaces { Début = false, Fin = true, Successif = false }; } }
        /// <summary>
        /// Retourne un objet SansEspaces qui enlève les char WhiteSpace au début et à la fin d'une string.
        /// </summary>
        public static SansEspaces AuDébutNiALaFin { get { return new SansEspaces { Début = true, Fin = true, Successif = false }; } }
        /// <summary>
        /// Retourne un objet SansEspaces qui remplace les char WhiteSpace successifs d'une string par un espace.
        /// </summary>
        public static SansEspaces Successifs { get { return new SansEspaces { Début = true, Fin = true, Successif = false }; } }
        /// <summary>
        /// Retourne un objet SansEspaces qui enlève les char WhiteSpace au début et à la fin d'une string
        /// et qui remplace les char WhiteSpace successifs par un espace.
        /// </summary>
        public static SansEspaces AuDébutNiALaFinNiSuccessifs { get { return new SansEspaces { Début = true, Fin = true, Successif = true }; } }

        /// <summary>
        /// Enlève les char WhiteSpace au début et/ou à la fin d'une string et/ou remplace les char WhiteSpace successifs par un espace.
        /// </summary>
        /// <param name="entrée">string à traiter.</param>
        /// <returns>la string obtenue en enlevant les char WhiteSpace si l'entrée n'est pas null ou vide, l'entrée sinon.</returns>
        private string _Traite(string entrée)
        {
            if (Début)
            {
                entrée = entrée.TrimStart();
            }
            if (Fin)
            {
                entrée = entrée.TrimEnd();
            }
            if (Successif)
            {
                char[] sortie = new char[entrée.Length];
                bool aprésEspace = false;
                int i = 0;
                foreach (char c in entrée.ToCharArray())
                {
                    if (char.IsWhiteSpace(c))
                    {
                        if (!aprésEspace)
                        {
                            // c'est le premier WhiteSpace qui suit un char qui ne soit pas un WhiteSpace
                            // on le remplace par un espace
                            sortie[i++] = ' ';
                            aprésEspace = true;
                        }
                        else
                        {
                            // c'est un WhiteSpace qui suit un WhiteSpace donc il faut sauter ce char
                            continue;
                        }

                        sortie[i++] = c;
                        aprésEspace = true;
                        continue;
                    }
                    // le char n'est pas un WhiteSpace on l'ajoute à la sortie
                    sortie[i++] = c;
                    // le char suivant ne succèdera pas à un WhiteSpace
                    aprésEspace = false;
                }
                return new string(sortie, 0, i);
            }
            else
            {
                return entrée;
            }
        }

        /// <summary>
        /// Enlève les char WhiteSpace au début et/ou à la fin d'une string et/ou remplace les char WhiteSpace successifs par un espace.
        /// </summary>
        /// <param name="entrée">string à traiter.</param>
        /// <returns>la string obtenue en enlevant les char WhiteSpace si l'entrée n'est pas null ou vide, l'entrée sinon.</returns>
        public string Traite(string entrée)
        {
            if (string.IsNullOrEmpty(entrée))
            {
                return entrée;
            }
            return _Traite(entrée);
        }

        /// <summary>
        /// Fixe les valeurs de propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// Les noms des propriétés dont la valeur après traitement est null ou vide sont ajoutés à NomsDesPropriétésNullesOuVides.
        /// </summary>
        /// <param name="objet">Object ayant les propriétés à traiter. Si null, une exception est levée.</param>
        /// <param name="àVérifier">Array d'objets contenant les noms des propriétés à traiter et les actions à effectuer si la valeur
        /// de la propriété est nulle ou vide.
        /// Si pour l'un des noms l'objet n'a pas de propriété de ce nom ou si la propriété n'est pas de type string, une exception est levée.</param>
        public void FixeValeur(object objet, SansEspacesPropertyDef[] àVérifier)
        {
            if (objet == null)
            {
                throw new SansEspacesException("L'objet est null.");
            }
            foreach (SansEspacesPropertyDef def in àVérifier)
            {
                PropertyInfo property = objet.GetType().GetProperty(def.Nom);
                if (property == null)
                {
                    throw new SansEspacesException("La propriété n'existe pas.");
                }
                if (property.GetType() != typeof(string))
                {
                    throw new SansEspacesException("Le type de la propriété n'est pas string.");
                }
                string entrée = (string)property.GetValue(objet);
                if (entrée == null)
                {
                    if (def.QuandNull != null)
                    {
                        def.QuandNull(property.Name);
                    }
                    return;
                }
                string sortie = _Traite(entrée);
                if (def.QuandVide != null && string.IsNullOrEmpty(sortie))
                {
                    def.QuandVide(property.Name);
                }
                property.SetValue(objet, sortie);
            }
        }

        /// <summary>
        /// Fixe les valeurs de toutes les propriétés de type string non nulles d'un objet en enlevant les char WhiteSpace au début et/ou à la fin des valeurs
        /// de ces propriétés et/ou en remplaçant dans ces valeurs les char WhiteSpace successifs par un espace.
        /// </summary>
        /// <param name="objet">Object à traiter. Si null, une exception est levée.</param>
        public void FixeValeur(Object objet)
        {
            if (objet == null)
            {
                throw new SansEspacesException("L'objet est null.");
            }
            IEnumerable<PropertyInfo> properties = objet.GetType()
                .GetProperties()
                .Where(p => p.GetType() == typeof(string));
            foreach(PropertyInfo property in properties)
            {
                if (property == null)
                {
                    throw new SansEspacesException("La propriété n'existe pas.");
                }
                if (property.GetType() != typeof(string))
                {
                    throw new SansEspacesException("Le type de la propriété n'est pas string.");
                }
                string entrée = (string)property.GetValue(objet);
                string sortie = _Traite(entrée);
                property.SetValue(objet, sortie);
            }
        }

    }
}
