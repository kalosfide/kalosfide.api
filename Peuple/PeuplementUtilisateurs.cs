using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Fournisseurs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public static class PeuplementUtilisateurs
    {
        public static EnregistrementFournisseurVue[] Fournisseurs = new EnregistrementFournisseurVue[]
        {
            new EnregistrementFournisseurVue
            {
                Email = "f1@f",
                Password = "123456",
                Nom = "Les jardins du prévôt",
                Adresse = "Vellorgues, 84800 L'Isle sur la Sorgue",
                NomSite = "ljdp",
                Titre = "Les jardins du prévôt",
            },
            new EnregistrementFournisseurVue
            {
                Email = "f2@f",
                Password = "123456",
                Nom = "Le grand magasin",
                Adresse = "1 rue du magasin, Enville",
                NomSite = "le_grand_magasin",
                Titre = "Le grand magasin",
            },
        };

        public static EnregistrementClientVue[] ClientsAvecCompte = new EnregistrementClientVue[]
        {
            new EnregistrementClientVue
            {
                Email = "c1@c",
                Password = "123456",
                Nom = "Le grand Restaurant",
                Adresse = "2 rue Germain",
                SiteUid = "1",
                SiteRno = 1
            },
            new EnregistrementClientVue
            {
                Email = "c2@c",
                Password = "123456",
                Nom = "Restaurant Petitjean",
                Adresse = "1, rue Petitjean, Enville",
                SiteUid = "1",
                SiteRno = 1
            },
            new EnregistrementClientVue
            {
                Email = "c3@c",
                Password = "123456",
                Nom = "Chez nous",
                Adresse = "5 rue du magasin, Enville",
                SiteUid = "2",
                SiteRno = 1
            },
        };

        public static ClientVue Client(int no)
        {
            return new ClientVue
            {
                Nom = "Client" + no,
                Adresse = "Adresse" + no,
            };
        }

    }
}
