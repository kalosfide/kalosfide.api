using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Commandes
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICommandeService, CommandeService>();
        }
    }
}
