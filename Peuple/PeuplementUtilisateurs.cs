using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using System.Collections.Generic;

namespace KalosfideAPI.Peuple
{
    public static class PeuplementUtilisateurs
    {
        public static PeupleFournisseurVue[] Fournisseurs = new PeupleFournisseurVue[]
        {
            new PeupleFournisseurVue
            {
                Email = "f1@f",
                Password = "123456",
                Nom = "Les jardins du prévôt",
                Adresse = "Vellorgues",
                Ville = "84800 L'Isle sur la Sorgue",
                Url = "ljdp",
                Titre = "Les jardins du prévôt",
                ClientsSansCompte = 25,
                Produits = 150,
                Catégories = 7,
                Clients = new PeupleClientVue[]
                {
                    new PeupleClientVue
                    {
                        Email = "c1@c",
                        Password = "123456",
                        Nom = "Le grand Restaurant",
                        Adresse = "2 rue Germain",
                        Ville = "84800 L'Isle sur la Sorgue",
                    },
                    new PeupleClientVue
                    {
                        Email = "c2@c",
                        Password = "123456",
                        Nom = "Restaurant Petitjean",
                        Adresse = "1, rue Petitjean",
                        Ville = "Enville",
                    },
                    new PeupleClientVue
                    {
                        Email = "c3@c",
                        Password = "123456",
                        Nom = "Chez nous",
                        Adresse = "5 rue du magasin",
                        Ville = "Enville",
                    },
                    new PeupleClientVue
                    {
                        Email = "f2@f",
                        Password = "123456",
                        Nom = "Le grand magasin",
                        Adresse = "1 rue du magasin, Enville",
                        Ville = "Enville",
                    },

                }
            },
            new PeupleFournisseurVue
            {
                Email = "f2@f",
                Password = "123456",
                Nom = "Le grand magasin",
                Adresse = "1 rue du magasin, Enville",
                Ville = "Enville",
                Url = "le_grand_magasin",
                Titre = "Le grand magasin",
            },
        };

        public static PeupleClientVue[] ClientsAvecCompte = new PeupleClientVue[]
        {
        };

        public static ClientAEditer Client(int no)
        {
            return new ClientAEditer
            {
                Nom = "Client" + no,
                Adresse = "Adresse" + no,
                Etat = EtatRole.Actif
            };
        }

    }
}
