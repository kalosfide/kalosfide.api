using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{
    public class Tarif
    {
        public List<TarifProduit> Produits { get; set; }
        public List<TarifCatégorie> Catégories { get; set; }
    }
}
