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
        Produit Produit(uint idSite, uint idCatégorie, uint id)
        {
            Random random = new Random();
            Array types = Enum.GetValues(typeof(TypeMesure));
            TypeMesure typeMesure = (TypeMesure)types.GetValue(random.Next(types.Length));
            types = Enum.GetValues(typeof(TypeCommande));
            TypeCommande typeCommande = (TypeCommande)types.GetValue(random.Next(types.Length));
            decimal prix = .01m * Prix[random.Next(Prix.Length - 1)];
            bool disponible = random.Next(100) >= 5;
            Produit produit = new Produit
            {
                Id = id,
                SiteId = idSite,
                CategorieId = idCatégorie,
                Nom = "Produit" + id,
                TypeMesure = typeMesure,
                TypeCommande = typeCommande,
                Prix = prix,
                Disponible = disponible
            };
            return produit;
        }

        Catégorie Catégorie(uint idSite, uint id)
        {
            return new Catégorie
            {
                Id = id,
                SiteId = idSite,
                Nom = "Catégorie" + id
            };
        }

        public PeuplementCatalogue(uint idSite, int nbCatégories, int nbProduits, PeupleId peuplement)
        {
            Catégories = new List<Catégorie>();
            Produits = new List<Produit>();

            uint id = peuplement.Catégorie + 1;
            for (int i = 0; i < nbCatégories; i++, id++)
            {
                Catégories.Add(Catégorie(idSite, id));
            }

            Random random = new Random();

            id = peuplement.Produit + 1;
            int nbACréer = nbProduits;
            List<uint> idVides = Catégories.Select(c => c.Id).ToList();
            while (nbACréer > 0)
            {
                int nbVides = idVides.Count();
                uint idCatégorie;
                if (nbACréer < 2 * nbVides)
                {
                    // il faut choisir une vide
                    idCatégorie = idVides.ElementAt(random.Next(nbVides - 1));
                    idVides.Remove(idCatégorie);
                }
                else
                {
                    idCatégorie = (uint)(random.Next((int)(nbCatégories - 1)) + 1);
                }
                Produits.Add(Produit(idSite, idCatégorie, (uint)id));
                id++;
                nbACréer--;
            }
            peuplement.Catégorie = Catégories.Last().Id;
            peuplement.Produit = Produits.Last().Id;
        }
    }
}
