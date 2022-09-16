using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class LigneCLFTarif
    {
        public IEnumerable<LigneCLF> Lignes { get; set; }
        public ProduitAEnvoyer Produit { get; set; }
        public DateTime Date { get; set; }
    }
}
