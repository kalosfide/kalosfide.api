using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFPdfLigne
    {
        public string Catégorie { get; private set; }
        public string Produit { get; private set; }
        public decimal Prix { get; private set; }
        public decimal Quantité { get; private set; }
        public decimal Coût { get; private set; }
        public string TextePrix { get; private set; }
        public string TexteQuantité { get; private set; }
        public string TexteUnités { get; private set; }
        public string TexteCoût { get; private set; }

        public DateTime? DateProduit { get; set; }

        public CLFPdfLigne(LigneCLF ligne)
        {
            Catégorie = ligne.Produit.Catégorie.Nom;
            Produit = ligne.Produit.Nom;
            TextePrix = Data.Produit.PrixAvecLUnité(ligne.Produit);
            int quantité = (int)ligne.Quantité.Value;
            TexteQuantité = quantité == ligne.Quantité ? quantité.ToString() : string.Format(CultureInfo.CurrentCulture, "{0}", ligne.Quantité);
            if (ligne.Type == TypeCLF.Commande && ligne.Produit.SCALP == true)
            {
                TexteUnités = " pièce";
                if (ligne.Quantité > 1)
                {
                    TexteUnités += "s";
                }
            }
            Coût = ligne.Produit.Prix * ligne.Quantité.Value;
            TexteCoût = string.Format(CultureInfo.CurrentCulture, "{0:C2}", Coût);
        }

    }
}
