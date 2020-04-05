using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Roles
{
    class GèreArchive : Partages.KeyParams.GéreArchive<Role, RoleVue, ArchiveRole>
    {
        public GèreArchive(DbSet<Role> dbSet, DbSet<ArchiveRole> dbSetArchive) : base(dbSet, dbSetArchive)
        { }

        protected override ArchiveRole CréeArchive()
        {
            return new ArchiveRole();
        }

        protected override void CopieDonnéeDansArchive(Role donnée, ArchiveRole archive)
        {
            archive.Etat = donnée.Etat ?? TypeEtatRole.Nouveau;
        }

        protected override ArchiveRole CréeArchiveDesDifférences(Role donnée, RoleVue vue)
        {
            bool modifié = false;
            ArchiveRole archive = new ArchiveRole();
            if (vue.Etat != null && donnée.Etat != vue.Etat)
            {
                donnée.Etat = vue.Etat;
                archive.Etat = vue.Etat;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class RoleService : Partages.KeyParams.KeyUidRnoService<Role, RoleVue>, IRoleService
    {

        public RoleService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Role;
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveRole);
            _inclutRelations = Complète;
        }

        /// <summary>
        /// retourne le site d'un objet ayant un UidRno qui est celui du role qui en est propriétaire
        /// </summary>
        /// <param name="akeyRole">Role ou Client ou Fournisseur ou Commande ou DétailCommande ou Livraison </param>
        /// <returns></returns>
        public async Task<Site> SiteDeRole(AKeyBase akeyRole)
        {
            Role role = await _context.Role.Where(r => r.CommenceKey(akeyRole.KeyParam)).FirstOrDefaultAsync();
            return role?.Site;
        }

        public async Task<List<Role>> RolesDuSite(AKeyUidRno aKeySite)
        {
            List<Role> roles = await _context.Role
                .Where(r => r.SiteUid == aKeySite.Uid && r.SiteRno == aKeySite.Rno)
                .ToListAsync();
            return roles;
        }

        public bool TempsInactifEcoulé(DateTime date)
        {
            TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
            long durée = DateTime.Now.Ticks - date.Ticks;
            if (timeSpan.TotalDays > TypeEtatRole.JoursInactifAvantExclu())
            {
                return true;
            }
            return false;
        }

        public async Task<bool> TempsInactifEcoulé(Role role)
        {
            if (role.Etat == TypeEtatRole.Inactif)
            {
                ArchiveRole archive = await _context.ArchiveRole
                    .Where(er => role.AMêmeKey(er))
                    .LastOrDefaultAsync();
                long durée = DateTime.Now.Ticks - archive.Date.Ticks;
                if (durée >= TypeEtatRole.TicksInactifAvantExclu())
                {
                    return true;
                }
            }
            return false;
        }

        IQueryable<Role> Complète(IQueryable<Role> données)
        {
            return données.Include(d => d.Site);
        }

        public void ChangeEtatSansSauver(Role role, string état)
        {
            role.Etat = état;
            _context.Role.Update(role);
            ArchiveRole etatRole = new ArchiveRole
            {
                Date = DateTime.Now,
                Etat = état
            };
            etatRole.CopieKey(role.KeyParam);
            _context.ArchiveRole.Add(etatRole);
        }

        public async Task<RetourDeService<Role>> ChangeEtat(Role role, string état)
        {
            ChangeEtatSansSauver(role, état);
            return await SaveChangesAsync(role);
        }

        public override Role CréeDonnée()
        {
            return new Role();
        }

        public async Task<Role> CréeRole(Utilisateur utilisateur)
        {
            int roleNo = await DernierNo(utilisateur.Uid) + 1;
            Role role = new Role
            {
                Uid = utilisateur.Uid,
                Rno = roleNo,
                Etat = TypeEtatRole.Nouveau
            };
            return role;
        }

        public override RoleVue CréeVue(Role donnée)
        {
            RoleVue vue = new RoleVue
            {
                SiteUid = donnée.SiteUid,
                SiteRno = donnée.SiteRno,
                NomSite = donnée.Site.NomSite,
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }

        public override void CopieVueDansDonnée(Role donnée, RoleVue vue)
        {
        }

        public override void CopieVuePartielleDansDonnée(Role donnée, RoleVue vue, Role donnéePourComplèter)
        {
        }
    }
}