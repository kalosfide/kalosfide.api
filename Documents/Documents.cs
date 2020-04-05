using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Documents
{
    public class Documents
    {
        public List<DocumentCommande> Commandes { get; set; }
        public List<DocumentBilan> Livraisons { get; set; }
        public List<DocumentBilan> Factures { get; set; }
    }
}
