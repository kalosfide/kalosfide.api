using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    /// <summary>
    /// CRU = Client Role Utilisateur
    /// </summary>
    public class CRU
    {
        public Client Client { get; set; }
        public Role Role { get; set; }
        public DateTime DateEtat { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public bool AvecCommandes { get; set; }
    }
}
