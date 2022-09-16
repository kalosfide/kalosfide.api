using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public class KeyDoc: KeyDocSansType
    {

        /// <summary>
        /// L'une des constantes TypeCLF.Commande ou TypeCLF.Livraison ou TypeCLF.Facture
        /// </summary>
        public TypeCLF Type { get; set; }

        /// <summary>
        /// Date du téléchargement du document.
        /// </summary>
        public DateTime? Téléchargé { get; set; }

    }
}
