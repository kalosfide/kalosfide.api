using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace KalosfideAPI.Sécurité
{

    public static class JwtClaims
    {
        public const string UserId = "usid";
        public const string UserName = "usna";

        public const string UtilisateurId = "utid";
        public const string EtatUtilisateur = "etut";

        public const string Roles = "rols";

        public const string RoleId = "roid";
        public const string EtatRole = "etro";
        public const string UrlSite = "ursi";
    }
}
