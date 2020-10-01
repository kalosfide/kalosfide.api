using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Démarrage;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Sécurité;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KalosfideAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            BaseDeDonnées.ConfigureService(services, Configuration);

            ServicesDeDonnées.ConfigureServices(services);

            Authentification.ConfigureServices(services, Configuration);

            services.AddCors(options =>
            {
            options.AddPolicy("AutoriseLocalhost",
                builder => builder
                    .WithOrigins("https://localhost:4200", "https://localhost:44391")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders(JwtFabrique.NomJwtRéponse.ToLower())
                    .Build()
                );
            });

            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddDataProtection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable IDE0060 // Supprimer le paramètre inutilisé
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
#pragma warning restore IDE0060 // Supprimer le paramètre inutilisé
        {
            app.UseMiddleware<ErrorWrappingMiddleware>();


            // 
            BaseDeDonnées.Configure(Configuration, env, app.ApplicationServices);


            app.UseDefaultFiles();

            app.UseStaticFiles();

            // Cross - Origin Read Blocking(CORB) blocked cross-origin response
            app.UseCors("AutoriseLocalhost");

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{param?}"
                    );
                routes.MapRoute(
                    name: "UidRno",
                    template: "{controller}/{action}/{uid?}/{rno?}"
                    );
                routes.MapRoute(
                    name: "Site",
                    template: "{siteId}/{controller}/{action}/{uid?}/{rno?}"
                    );
            });
        }
    }
}
