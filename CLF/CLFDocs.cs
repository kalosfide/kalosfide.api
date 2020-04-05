using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFDocs
    {
        /// <summary>
        /// Envoyé avec Etat seulement pour vérifier s'il faut recharger le stock
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        /// Envoyé avec Date seulement pour vérifier s'il faut recharger le stock
        /// </summary>
        public Catalogue Catalogue { get; set; }

        /// <summary>
        /// Le Client reçu avec les documents à affecter à une synthèse ne contient que la clé.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Reçu avec les documents contenant seulement leur numéros pour les affecter à une synthèse.
        /// Envoyé avec les lignes ou le résumé.
        /// </summary>
        public List<CLFDoc> Documents { get; set; }
    }

    public class CLFDocsSynthèse
    {
        public KeyUidRno KeyClient { get; set; }
        public List<long> NoDocs { get; set; }
    }
}
