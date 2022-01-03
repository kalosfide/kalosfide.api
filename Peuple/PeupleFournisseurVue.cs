using KalosfideAPI.Data;
using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;
using System.Collections.Generic;

namespace KalosfideAPI.Peuple
{
    public class PeupleFournisseurVue: ICréeCompteVue, IRoleData, ISiteData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }

        public string Url { get; set; }
        public string Titre { get; set; }

        public PeupleClientVue[] Clients { get; set; }

        public int? ClientsSansCompte { get; set; }
        public int? Produits { get; set; }
        public int? Catégories { get; set; }
    }
}
