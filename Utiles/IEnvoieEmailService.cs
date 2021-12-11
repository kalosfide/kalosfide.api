using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public interface IEnvoieEmailService
    {
        Task EnvoieEmail(string email, string objet, string message);
        Task EnvoieEmail(string email, string objet, string message, string urlBase, string àEncoder, Dictionary<string, string> urlParams);
        Task EnvoieEmail<T>(string email, string objet, string message, string urlBase, T àEncoder, Dictionary<string, string> urlParams);
        T DécodeCodeDeEmail<T>(string code);
    }
}
