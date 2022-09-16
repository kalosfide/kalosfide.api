using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public class HasardException : Exception
    {
        public HasardException(string message) : base(message) { }
    }

    public class ItemAvecPoids<T>
    {
        public T Item { get; private set; }
        public int Poids { get; private set; }

        public int Cumul { get; set; }

        public ItemAvecPoids(T item, int poids)
        {
            Item = item;
            Poids = poids;
        }
    }
    class HasardItem<T>
    {
        public T Item { get; set; }
        public double PoidsRelatifCumulé { get; set; }
    }
    public class Hasard<T>
    {
        private readonly List<HasardItem<T>> items;
        private readonly Random random;

        public Hasard(List<ItemAvecPoids<T>> itemsAvecPoids)
        {
            if (itemsAvecPoids.Count == 0)
            {
                items = new List<HasardItem<T>>();
                return;
            }
            itemsAvecPoids[0].Cumul = itemsAvecPoids[0].Poids;
                if (itemsAvecPoids[0].Poids <= 0)
                {
                    throw new HasardException(string.Format("La poids au rang {0} est négative ou nulle.", 1));
                }
            for (int i = 1; i < itemsAvecPoids.Count; i++)
            {
                if (itemsAvecPoids[i].Poids <= 0)
                {
                    throw new HasardException(string.Format("La poids au rang {0} est négative ou nulle.", i + 1));
                }
                itemsAvecPoids[i].Cumul = itemsAvecPoids[i].Poids + itemsAvecPoids[i - 1].Cumul;
            }
            int cumul = itemsAvecPoids.Last().Cumul;
            items = itemsAvecPoids.Select((ItemAvecPoids<T> itemAvecPoids) => new HasardItem<T>
            {
                Item = itemAvecPoids.Item,
                PoidsRelatifCumulé = ((double)itemAvecPoids.Cumul) / cumul
            }).ToList();
            random = new Random();
        }

        public T Suivant()
        {
            double tirage = random.NextDouble();
            for (int i = 0; i < items.Count; i++)
            {
                HasardItem<T> hasardItem = items[i];
                if (tirage < hasardItem.PoidsRelatifCumulé)
                {
                    return hasardItem.Item;
                }
            }
            return items.Last().Item;
        }
    }
}
