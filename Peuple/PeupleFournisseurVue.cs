using KalosfideAPI.Sites;
using KalosfideAPI.Utilisateurs;

namespace KalosfideAPI.Peuple
{
    public class PeupleFournisseurVue: ICréeCompteVue, ICréeSiteVue
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Etat { get; set; }

        public string Url { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

    }
}
