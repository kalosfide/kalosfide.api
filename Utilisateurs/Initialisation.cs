using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public static class Initialisation
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUtilisateurService, UtilisateurService>();
        }
    }
}

