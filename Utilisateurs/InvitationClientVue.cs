using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{

    /// <summary>
    /// Données envoyées à la page Devenir client contenant le client de l'invitation
    /// </summary>
    public class InvitationClientVue
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Url { get; set; }
        public string Titre { get; set; }
    }
}
