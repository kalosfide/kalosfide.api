using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Utiles
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUtileService, UtileService>();
        }

    }
}
