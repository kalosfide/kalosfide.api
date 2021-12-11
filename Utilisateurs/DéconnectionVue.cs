using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public class DéconnectionVue
    {
        /// <summary>
        /// Rno du role correspondant au site de fournisseur où l'utilisateur était lors de sa déconnection.
        /// 0 si l'utilisateur n'était pas sur un site de fournisseur lors de sa déconnection.
        /// </summary>
        public int NoDernierRole { get; set; }
    }
}
