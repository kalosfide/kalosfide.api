using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public interface ICréeSiteVue
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Url { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }
    }

    public class CréeSiteVue: ICréeSiteVue
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Url { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }
    }
}
