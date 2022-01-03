using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    /// <summary>
    /// Contient les Ids des dernières entités créées de chaque type.
    /// </summary>
    public class PeupleId
    {
        /// <summary>
        /// Id du dernier Fournisseur créé.
        /// </summary>
        public uint Fournisseur { get; set; }

        /// <summary>
        /// Id du dernier Client créé.
        /// </summary>
        public uint Client { get; set; }

        /// <summary>
        /// Id du dernier Produit créé.
        /// </summary>
        public uint Produit { get; set; }

        /// <summary>
        /// Id de la dernière Catégorie créée.
        /// </summary>
        public uint Catégorie { get; set; }

        public PeupleId()
        {
            Fournisseur = 0;
            Client = 0;
            Catégorie = 0;
            Produit = 0;
        }
    }
}
