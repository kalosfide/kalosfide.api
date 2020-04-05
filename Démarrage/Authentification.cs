using KalosfideAPI.Data;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalosfideAPI.Démarrage
{
    public class Authentification
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Get options from app settings
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtFabriqueOptions));

            string SecretKey = jwtAppSettingOptions[nameof(SecretKey)];
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

            services.AddSingleton<IJwtFabrique, JwtFabrique>();

            // Configure JwtIssuerOptions
            services.Configure<JwtFabriqueOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtFabriqueOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtFabriqueOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtFabriqueOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtFabriqueOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = false;
                // developpement identity options
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequiredLength = 6;
            })
               .AddEntityFrameworkStores<ApplicationContext>()
;//               .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.ClaimsIssuer = tokenValidationParameters.ValidIssuer;
                jwtBearerOptions.TokenValidationParameters = tokenValidationParameters;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.Events = new JwtEventsHandlers();
            });
   
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/api/erreur/403";
                options.Events.OnRedirectToAccessDenied =
                    context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api")
                        && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                            return Task.FromResult<object>(null);
                        }
                    };
                options.LoginPath = "/api/erreur/401";
                options.Events.OnRedirectToLogin =
                    context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api")
                        && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                            return Task.FromResult<object>(null);
                        }
                    };

            });
        }
    }
}