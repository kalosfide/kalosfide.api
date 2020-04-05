using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.DétailCommandes
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDétailCommandeService, DétailCommandeService>();
        }
    }
}
