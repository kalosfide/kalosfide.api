using KalosfideAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class JwtRéponse
    {
        public string Id { get; set; }
        public string Jeton { get; set; }
        public long ExpireDans { get; set; }
    }

    public class JwtFabrique : IJwtFabrique
    {

        public static string NomJwtRéponse = "JwtBearer";

        private readonly JwtFabriqueOptions _jwtOptions;

        public JwtFabrique(IOptions<JwtFabriqueOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<JwtRéponse> CréeReponse(CarteUtilisateur carte)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, carte.Utilisateur.ApplicationUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaims.UserId, carte.Utilisateur.UserId),
                new Claim(JwtClaims.UserName, carte.Utilisateur.ApplicationUser.UserName),
                new Claim(JwtClaims.UtilisateurId, carte.Utilisateur.Uid),
                new Claim(JwtClaims.SessionId, carte.Utilisateur.SessionId.ToString())
            };

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims.ToArray(),
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            string jeton = new JwtSecurityTokenHandler().WriteToken(jwt);

            JwtRéponse jwtr = new JwtRéponse
            {
                Id = carte.Utilisateur.UserId,
                Jeton = jeton,
                ExpireDans = (int)_jwtOptions.ValidFor.TotalSeconds,
            };
            return jwtr;
        }

        // Utilités

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtFabriqueOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtFabriqueOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtFabriqueOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtFabriqueOptions.JtiGenerator));
            }
        }
    }
}