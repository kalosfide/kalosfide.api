using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Documents
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDocumentService, DocumentService>();
        }
    }
}
