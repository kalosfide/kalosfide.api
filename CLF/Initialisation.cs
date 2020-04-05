using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.CLF
{
    public static class Initialisation
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICLFService, CLFService>();
        }
    }
}
