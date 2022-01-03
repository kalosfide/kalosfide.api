using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    /// <summary>
    /// Contient la key et le nom du client et la date d'un document.
    /// </summary>
    public class CLFChercheDoc
    {
        /// <summary>
        /// Id du client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Nom du client
        /// </summary>
        public string Nom { get; set; }
        /// <summary>
        /// Date du document
        /// </summary>
        public DateTime Date { get; set; }
    }
}
