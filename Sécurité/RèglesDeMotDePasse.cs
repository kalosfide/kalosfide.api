using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class RèglesDeMotDePasse
    {
        public bool NoSpaces;
        public int RequiredLength;
        public bool RequireDigit;
        public bool RequireLowercase;
        public bool RequireUppercase;
        public bool RequireNonAlphanumeric;
    }
}
