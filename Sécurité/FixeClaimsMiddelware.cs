using KalosfideAPI.Data;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class FixeClaimsMiddelware : IMiddleware
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtFabrique _jwtFabrique;
        private readonly IUtilisateurService _service;

        public FixeClaimsMiddelware(IJwtFabrique jwtFabrique, IUtilisateurService service, UserManager<ApplicationUser> userManager)
        {
            _jwtFabrique = jwtFabrique;
            _service = service;
            _userManager = userManager;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var claims = context.User.Claims;

            await next(context);

            claims = context.User.Claims;
            if (context.User.Identity.IsAuthenticated)
            {
            }
            claims = context.User.Claims;
        }
    }
}
