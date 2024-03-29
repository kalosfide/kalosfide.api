﻿using Microsoft.Extensions.DependencyInjection;

namespace KalosfideAPI.Démarrage
{
    public class ServicesDeDonnées
    {
        public static void ConfigureServices(IServiceCollection services)
        {

            Utilisateurs.Initialisation.ConfigureServices(services);

            Utiles.Initialisation.ConfigureServices(services);

            Sites.Initialisation.ConfigureServices(services);

            Fournisseurs.Initialisation.ConfigureServices(services);

            Clients.Initialisation.ConfigureServices(services);

            Admin.Initialisation.ConfigureServices(services);

            Catégories.Initialisation.ConfigureServices(services);

            Produits.Initialisation.ConfigureServices(services);


            Catalogues.Initialisation.ConfigureServices(services);

            Peuple.Initialisation.ConfigureServices(services);


            CLF.Initialisation.ConfigureServices(services);

            Préférences.Initialisation.ConfigureServices(services);
        }
    }
}
