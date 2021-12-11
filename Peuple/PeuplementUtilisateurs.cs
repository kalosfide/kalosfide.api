using KalosfideAPI.Clients;
using KalosfideAPI.Data.Constantes;

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
                Adresse = "Vellorgues, 84800 L'Isle sur la Sorgue",
                Etat = TypeEtatRole.Nouveau,
                Url = "ljdp",
                Titre = "Les jardins du prévôt",
            },
            new PeupleFournisseurVue
            {
                Email = "f2@f",
                Password = "123456",
                Nom = "Le grand magasin",
                Adresse = "1 rue du magasin, Enville",
                Etat = TypeEtatRole.Nouveau,
                Url = "le_grand_magasin",
                Titre = "Le grand magasin",
            },
        };

        public static PeupleClientVue[] ClientsAvecCompte = new PeupleClientVue[]
        {
            new PeupleClientVue
            {
                Email = "c1@c",
                Password = "123456",
                Nom = "Le grand Restaurant",
                Adresse = "2 rue Germain",
                Etat = TypeEtatRole.Nouveau,
                SiteUid = "1",
                SiteRno = 1
            },
            new PeupleClientVue
            {
                Email = "c2@c",
                Password = "123456",
                Nom = "Restaurant Petitjean",
                Adresse = "1, rue Petitjean, Enville",
                Etat = TypeEtatRole.Nouveau,
                SiteUid = "1",
                SiteRno = 1
            },
            new PeupleClientVue
            {
                Email = "c3@c",
                Password = "123456",
                Nom = "Chez nous",
                Adresse = "5 rue du magasin, Enville",
                Etat = TypeEtatRole.Nouveau,
                SiteUid = "1",
                SiteRno = 1
            },
        };

        public static ClientVue Client(int no)
        {
            return new ClientVue
            {
                Nom = "Client" + no,
                Adresse = "Adresse" + no,
                Etat = TypeEtatRole.Actif
            };
        }

    }
}
