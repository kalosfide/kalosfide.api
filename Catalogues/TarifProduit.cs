using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public class TarifProduit
    {
        public long No { get; set; }

        public string Nom { get; set; }

        public DateTime Date { get; set; }

        public long CategorieNo { get; set; }

        public string TypeCommande { get; set; }
        public string TypeMesure { get; set; }
        public decimal? Prix { get; set; }
    }
}
