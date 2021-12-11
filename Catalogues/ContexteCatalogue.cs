using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public class ContexteCatalogue
    {
        /// <summary>
        /// TypeEtatSite.Catalogue quand une modification du catalogue est en cours.
        /// TypeEtatSite.Ouvert sinon
        /// </summary>
        public bool Ouvert { get; set; }

        /// <summary>
        /// Date du début de la modification du catalogue si le site est d'état Catalogue.
        /// Date de la fin de la dernière modification du catalogue si le site n'est pas d'état Catalogue.
        /// </summary>
        public DateTime? DateCatalogue { get; set; }

        public ContexteCatalogue(Site site)
        {
            Ouvert = site.Ouvert;
            DateCatalogue = site.DateCatalogue;
        }
    }
}
