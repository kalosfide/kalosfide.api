using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public class PeupleFournisseur
    {
        public Fournisseur Fournisseur { get; set; }
        public List<Client> Clients { get; set; }
        public List<Produit> Produits { get; set; }
    }
}
