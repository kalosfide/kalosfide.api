using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Commandes
{
    public class DétailsParProduit
    {
        public Produit Produit { get; set; }
        public List<DétailCommande> Détails { get; set; }
    }
}
