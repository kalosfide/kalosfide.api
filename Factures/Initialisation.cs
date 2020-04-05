using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Factures
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFactureService, FactureService>();
        }
    }
}
