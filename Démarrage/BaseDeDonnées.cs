using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KalosfideAPI.Démarrage
{
    public class BaseDeDonnées
    {

        public static void ConfigureService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options
                .UseLazyLoadingProxies()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        public static void Configure(IConfiguration configuration, IHostingEnvironment env, IServiceProvider svp)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            if (env.IsDevelopment()) optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            else if (env.IsStaging()) optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            else if (env.IsProduction()) optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        }

    }
}
