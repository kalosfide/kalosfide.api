using KalosfideAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KalosfideAPI.Sécurité
{
    [Route("api/[controller]/[action]")]
    public class MotDePasseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MotDePasseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Options()
        {
            var options = _userManager.Options.Password;
            var règles = new RèglesDeMotDePasse
            {
                NoSpaces = true,
                RequireDigit = options.RequireDigit,
                RequiredLength = options.RequiredLength,
                RequireLowercase = options.RequireLowercase,
                RequireUppercase = options.RequireUppercase,
                RequireNonAlphanumeric = options.RequireNonAlphanumeric
            };
            return Ok(règles);
        }
    }
}
