using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public class ProduitFabrique
    {
        public static void Copie(Produit produit, IProduitDataSansEtatNiDate data)
        {
            data.Nom = produit.Nom;
            data.CategorieNo = produit.CategorieNo;
            data.TypeCommande = produit.TypeCommande;
            data.TypeMesure = produit.TypeMesure;
            data.Prix = produit.Prix;
        }

        public static void Copie(Produit produit, IProduitDataSansDate data)
        {
            Copie(produit, (IProduitDataSansEtatNiDate)data);
            data.Etat = produit.Etat;
        }

        public static void CopieSiPasNullSansEtatNiDate(IProduitDataSansEtatNiDate de, IProduitDataSansEtatNiDate vers)
        {
            if (de.Nom != null) { vers.Nom = de.Nom; }
            if (de.CategorieNo != null) { vers.CategorieNo = de.CategorieNo.Value; }
            if (de.TypeCommande != null) { vers.TypeCommande = de.TypeCommande; }
            if (de.TypeMesure != null) { vers.TypeMesure = de.TypeMesure; }
            if (de.Prix != null) { vers.Prix = de.Prix.Value; }
        }

        public static void CopieSiPasNullSansDate(IProduitDataSansDate de, IProduitDataSansDate vers)
        {
            CopieSiPasNullSansEtatNiDate(de, vers);
            if (de.Etat != null) { vers.Etat = de.Etat; }
        }
    }
}
