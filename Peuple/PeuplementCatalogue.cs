using KalosfideAPI.Data;
using KalosfideAPI.Utiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Peuple
{
    public class PeuplementCatalogue
    {
        public List<Produit> Produits { get; private set; }
        public List<Catégorie> Catégories { get; private set; }

        static readonly int[] Prix = new int[]
            { 100, 150, 200, 250, 400, 500, 750, 800, 900, 1050 };

        private readonly Hasard<TypeMesure> hasardTypeMesure;
        private readonly Hasard<bool> hasardPSCALP;
        private readonly Hasard<bool> hasardDisponible;

        Produit Produit(uint idSite, uint idCatégorie, uint id)
        {
            Random random = new Random();
            TypeMesure typeMesure = hasardTypeMesure.Suivant();
            bool pSCALP = typeMesure == TypeMesure.Kilo ? hasardPSCALP.Suivant() : false;
            decimal prix = .01m * Prix[random.Next(Prix.Length - 1)];
            bool disponible = hasardDisponible.Suivant();
            Produit produit = new Produit
            {
                Id = id,
                SiteId = idSite,
                CategorieId = idCatégorie,
                Nom = "Produit" + id,
                TypeMesure = typeMesure,
                SCALP = pSCALP,
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
            hasardTypeMesure = new Hasard<TypeMesure>(new List<ItemAvecPoids<TypeMesure>>
            {
                new ItemAvecPoids<TypeMesure>(TypeMesure.Aucune, 10),
                new ItemAvecPoids<TypeMesure>(TypeMesure.Kilo, 5),
                new ItemAvecPoids<TypeMesure>(TypeMesure.Litre, 1),
            });
            hasardPSCALP = new Hasard<bool>(new List<ItemAvecPoids<bool>>
            {
                new ItemAvecPoids<bool>(true, 1),
                new ItemAvecPoids<bool>(false, 20)
            });
            hasardDisponible = new Hasard<bool>(new List<ItemAvecPoids<bool>>
            {
                new ItemAvecPoids<bool>(true, 95),
                new ItemAvecPoids<bool>(false, 5)
            });

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
