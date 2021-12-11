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
        /// Uid du client
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// Rno du client
        /// </summary>
        public long Rno { get; set; }
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
