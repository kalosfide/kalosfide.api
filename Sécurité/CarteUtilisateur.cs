using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Sites;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{

    [JsonObject(MemberSerialization.OptIn)]
    public class CarteRole
    {
        [JsonProperty]
        public int Rno { get; set; }
        [JsonProperty]
        public string Etat { get; set; }
        [JsonProperty]
        public string NomSite { get; set; }

        public Site Site { get; set; }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class CarteUtilisateur // pour l'Api et pour l'application cliente
    {
        [JsonProperty]
        public string UserId { get; set; }
        [JsonProperty]
        public string UserName { get; set; }

        [JsonProperty]
        public string Uid { get; set; }
        [JsonProperty]
        public string Etat { get; set; }

        [JsonProperty]
        public List<CarteRole> Roles { get; set; }

        [JsonProperty]
        public int NoDernierRole { get; set; }

        [JsonProperty]
        public List<SiteVue> Sites { get; set; }

        private readonly ApplicationContext _context;
        public CarteUtilisateur(ApplicationContext context)
        {
            _context = context;
        }

        public string RolesSérialisés
        {
            get
            {
                return JsonConvert.SerializeObject(Roles);
            }
            set
            {
                Roles = JsonConvert.DeserializeObject<List<CarteRole>>(value);
            }
        }

        public void DonneClaims(ClaimsPrincipal user)
        {
            IEnumerable<Claim> claims = user.Claims;
            UserId = (claims.Where(c => c.Type == JwtClaims.UserId).First())?.Value;
            UserName = (claims.Where(c => c.Type == JwtClaims.UserName).First())?.Value;
            Uid = (claims.Where(c => c.Type == JwtClaims.UtilisateurId).First())?.Value;
            Etat = (claims.Where(c => c.Type == JwtClaims.EtatUtilisateur).First())?.Value;
            RolesSérialisés = (claims.Where(c => c.Type == JwtClaims.Roles).First())?.Value;
        }

        /// <summary>
        /// retourne le numéro du dernier role utilisé archivé pour cet Uid
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        private async Task<int> NoDernierRoleArchivé(string Uid)
        {
            return await _context.ArchiveUtilisateur
                .Where(au => au.Uid == Uid && au.NoDernierRole.HasValue)
                .Select(au => au.NoDernierRole.Value)
                .LastOrDefaultAsync();
        }

        /// <summary>
        /// copie les données de l'utilisateur, crée les listes des roles et des sites dont l'utilisateur est propriétaire
        /// copie le numéro du dernier role utilisé archivé
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns></returns>
        public async Task FixeUtilisateur(Utilisateur utilisateur)
        {
            UserId = utilisateur.UserId;
            UserName = utilisateur.ApplicationUser.UserName;
            Uid = utilisateur.Uid;
            Etat = utilisateur.Etat;
            Roles = utilisateur.Roles.Select(r => new CarteRole
            {
                Rno = r.Rno,
                Etat = r.Etat,
                NomSite = r.Site.NomSite,
                Site = r.Site
            }).ToList();
            Sites = utilisateur.Roles
                .Where(r => r.SiteUid == utilisateur.Uid)
                .Select(r => CréeSiteVue(r.Site))
                .ToList();
            NoDernierRole = await NoDernierRoleArchivé(utilisateur.Uid);
        }

        private SiteVue CréeSiteVue(Site site)
        {
            SiteVue siteVue = new SiteVue();
            siteVue.Copie(site);
            return siteVue;
        }

        public bool EstIdentifié
        {
            get
            {
                return UserId != null;
            }
        }

        public bool EstUtilisateurActif
        {
            get
            {
                return EstIdentifié && Etat == TypeEtatUtilisateur.Actif || Etat == TypeEtatUtilisateur.Nouveau;
            }
        }

        public bool EstAdministrateur
        {
            get
            {
                return EstUtilisateurActif && Roles.Count == 0;
            }
        }

        public bool EstRoleActif(CarteRole role)
        {
             return EstUtilisateurActif && (role.Etat == TypeEtatRole.Actif || role.Etat == TypeEtatRole.Nouveau);
        }

        private CarteRole RoleActifEtAMêmeUidRno(KeyParam param)
        {
            if (EstUtilisateurActif && param.Uid == Uid)
            {
                return Roles.Where(role => EstRoleActif(role) && role.Rno == param.Rno).FirstOrDefault();
            }
            return null;
        }

        public async Task<bool> EstActifEtAMêmeUidRno(KeyParam param)
        {
            CarteRole carteRole = RoleActifEtAMêmeUidRno(param);
            if (carteRole == null)
            {
                return false;
            }
            if (carteRole.Rno != NoDernierRole)
            {
                ArchiveUtilisateur archiveUtilisateur = new ArchiveUtilisateur
                {
                    Uid = Uid,
                    Date = DateTime.Now,
                    NoDernierRole = carteRole.Rno
                };
                _context.ArchiveUtilisateur.Add(archiveUtilisateur);
                await _context.SaveChangesAsync();
                NoDernierRole = carteRole.Rno;
            }
            return true;
        }

        public bool EstClient(AKeyUidRno site)
        {
            return Roles.Where(role => EstRoleActif(role) && role.Site.Uid == site.Uid && role.Site.Rno == site.Rno).Any();
        }
    }
}
