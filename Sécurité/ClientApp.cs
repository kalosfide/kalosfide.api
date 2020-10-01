using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class ClientApp
    {
        const string BaseUrl = "https://localhost:4200/";

        public const string Nom = "Kalosfide";

        public const string Compte = "compte";

        public const string ConfirmeEmail = "confirmeEmail";

        public const string RéinitialiseMotDePasse = "réinitialiseMotDePasse";

        public const string ChangeEmail = "changeEmail";

        public const string ConfirmeChangeEmail = "confirmeChangeEmail";

        public const string DevenirClient = "devenirClient";

        public static string Url(params string[] segments)
        {
            return BaseUrl + "a/" + string.Join('/', segments);
        }
    }
}
