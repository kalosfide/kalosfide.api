using KalosfideAPI.Catalogues;
using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public class PeuplementCatalogue
    {
        public List<Produit> Produits { get; private set; }
        public List<Catégorie> Catégories { get; private set; }

        static readonly int[] Prix = new int[]
            { 100, 150, 200, 250, 400, 750, 800, 900, 1050 };
        Produit Produit(AKeyUidRno aKeySite, int catégorieNo, int no)
        {
            Random random = new Random();
            string typeMesure = random.Next(2) == 0 ? UnitéDeMesure.Kilo : UnitéDeMesure.Aucune;
            string typeCommande = typeMesure == UnitéDeMesure.Aucune ? TypeUnitéDeCommande.Unité
                : random.Next(9) == 0 ? TypeUnitéDeCommande.UnitéOuVrac : TypeUnitéDeCommande.Vrac;
            decimal prix = .01m * Prix[random.Next(Prix.Length - 1)];
            string état = random.Next(100) < 5 ? TypeEtatProduit.Indisponible : TypeEtatProduit.Disponible;
            Produit produit = new Produit
            {
                Uid = aKeySite.Uid,
                Rno = aKeySite.Rno,
                No = no,
                CategorieNo = catégorieNo,
                Nom = "Produit" + no,
                TypeMesure = typeMesure,
                TypeCommande = typeCommande,
                Prix = prix,
                Etat = état
            };
            return produit;
        }

        Catégorie Catégorie(AKeyUidRno aKeySite, int no)
        {
            return new Catégorie
            {
                Uid = aKeySite.Uid,
                Rno = aKeySite.Rno,
                No = no,
                Nom = "Catégorie" + no
            };
        }

        public PeuplementCatalogue(AKeyUidRno aKeySite, int nbCatégories, int nbProduits)
        {
            Catégories = new List<Catégorie>();
            Produits = new List<Produit>();
            for (int no = 1; no <= nbCatégories; no++)
            {
                Catégories.Add(Catégorie(aKeySite, no));
            }

            Random random = new Random();

            int[] répartition = new int[nbCatégories];
            for (int no = 1; no <= nbProduits; no++)
            {
                List<int> noVides = new List<int>();
                for (int i = 0; i < nbCatégories; i++)
                {
                    if (répartition[i] == 0)
                    {
                        noVides.Add(i + 1);
                    }
                }
                int nbVides = noVides.Count();
                int categorieNo;
                int nbACréer = nbProduits - no + 1;
                if (nbACréer < 2 * nbVides)
                {
                    // il faut choisir une vide
                    categorieNo = noVides.ElementAt(random.Next(nbVides - 1));
                }
                else
                {
                    categorieNo = random.Next(nbCatégories - 1) + 1;
                }
                Produits.Add(Produit(aKeySite, categorieNo, no));
            }
        }
    }
}
