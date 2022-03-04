using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public interface IEnvoieEmailService
    {
        Task EnvoieEmail(string email, string objet, string message);
        Task EnvoieEmail(string email, string objet, string message, string urlBase, string àEncoder, DateTime finValidité, List<KeyValuePair<string, string>> urlParams);
        Task EnvoieEmail(string email, string objet, string message, string urlBase, string àEncoder, List<KeyValuePair<string, string>> urlParams);
        Task EnvoieEmail<T>(string email, string objet, string message, string urlBase, T àEncoder, DateTime finValidité, List<KeyValuePair<string, string>> urlParams);
        Task EnvoieEmail<T>(string email, string objet, string message, string urlBase, T àEncoder, List<KeyValuePair<string, string>> urlParams);
        T DécodeCodeDeEmail<T>(string code);
    }
}
