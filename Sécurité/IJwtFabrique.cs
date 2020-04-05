using KalosfideAPI.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public interface IJwtFabrique
    {
        Task<JwtRéponse> CréeReponse(CarteUtilisateur carteUtilisateur);
    }
}
