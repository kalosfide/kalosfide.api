using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public static class Initialisation
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IClientService, ClientService>();
        }
    }
}
