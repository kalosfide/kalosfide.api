using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    /// <summary>
    /// RU = { Role, Utilisateur } d'un client
    /// </summary>
    public class RU
    {
        public Role Role { get; set; }
        public DateTime DateEtat { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public bool AvecDocuments { get; set; }
    }
}
