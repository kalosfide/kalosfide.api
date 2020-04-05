using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Catalogues
{
    public static class Initialisation
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICatalogueService, CatalogueService>();
        }
    }
}
