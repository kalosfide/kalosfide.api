using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public class EnvoieEmailService : IEnvoieEmailService
    {
        private readonly IDataProtector _protector;

        public EnvoieEmailService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("Kalosfide.devenir.client");
        }

#pragma warning disable IDE0060 // Supprimer le paramètre inutilisé
        private Task SendEmailAsync(string Email, string objet, string corps)
#pragma warning restore IDE0060 // Supprimer le paramètre inutilisé
        {
            return Task.CompletedTask;
        }

        public async Task EnvoieEmail(string email, string objet, string message)
        {
            await SendEmailAsync(email, objet, message);
        }

        public async Task EnvoieEmail(string email, string objet, string message, string urlBase, string àEncoder, Dictionary<string, string> urlParams)
        {
            string urlAvecParams = urlBase;
            if (àEncoder != null)
            {
                string token = _protector.Protect(àEncoder);
                string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                urlAvecParams += "?code=" + code;
            }
            if (urlParams != null)
            {
                foreach (KeyValuePair<string, string> param in urlParams)
                {
                    urlAvecParams += "&" + param.Key + "=" + param.Value;
                }
            }
            string corps = message + " en cliquant sur ce lien: <a href=\"" + urlAvecParams + "\">" + urlBase + "</a>";
            await SendEmailAsync(email, objet, corps);
        }

        public async Task EnvoieEmail<T>(string email, string objet, string message, string urlBase, T àEncoder, Dictionary<string, string> urlParams)
        {
            string texteAEncoder = JsonSerializer.Serialize(àEncoder);
            await EnvoieEmail(email, objet, message, urlBase, texteAEncoder, urlParams);
        }

        public T DécodeCodeDeEmail<T>(string code)
        {
            byte[] bytes = WebEncoders.Base64UrlDecode(code);
            string token = Encoding.UTF8.GetString(bytes);
            T décodé;
            try
            {
                string x = _protector.Unprotect(token);
                décodé = JsonSerializer.Deserialize<T>(x);
            }
            catch (Exception)
            {
                décodé = default;
            }
            return décodé;
        }

    }

}
