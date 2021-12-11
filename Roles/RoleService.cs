using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Roles
{
    class GèreArchive : Partages.KeyParams.GéreArchiveUidRno<Role, RoleVue, ArchiveRole>
    {
        public GèreArchive(
            DbSet<Role> dbSet,
            IIncludableQueryable<Role, ICollection<ArchiveRole>> query,
            Func<Role, ICollection<ArchiveRole>> archives,
            DbSet<ArchiveRole> dbSetArchive,
            IIncludableQueryable<ArchiveRole, Role> queryArchive,
            Func<ArchiveRole, Role> donnée
            ) : base(dbSet, query, archives, dbSetArchive, queryArchive, donnée)
        { }

        protected override ArchiveRole CréeArchive()
        {
            return new ArchiveRole();
        }

        protected override void CopieDonnéeDansArchive(Role donnée, ArchiveRole archive)
        {
            archive.Nom = donnée.Nom;
            archive.Adresse = donnée.Adresse;
            archive.Ville = donnée.Ville;
            archive.Etat = donnée.Etat;
        }

        protected override ArchiveRole CréeArchiveDesDifférences(Role donnée, RoleVue vue)
        {
            bool modifié = false;
            ArchiveRole archive = new ArchiveRole();
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                donnée.Nom = vue.Nom;
                archive.Nom = vue.Nom;
                modifié = true;
            }
            if (vue.Adresse != null && donnée.Adresse != vue.Adresse)
            {
                donnée.Adresse = vue.Adresse;
                archive.Adresse = vue.Adresse;
                modifié = true;
            }
            if (vue.Ville != null && donnée.Ville != vue.Ville)
            {
                donnée.Ville = vue.Ville;
                archive.Ville = vue.Ville;
                modifié = true;
            }
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
            _géreArchive = new GèreArchive(
                _dbSet, _dbSet.Include(r => r.Archives), (Role role) => role.Archives,
                _context.ArchiveRole, _context.ArchiveRole.Include(a => a.Role), (ArchiveRole archive) => archive.Role
                );
            _inclutRelations = Complète;
        }

        /// <summary>
        /// retourne le site d'un objet ayant un UidRno qui est celui du role qui en est propriétaire
        /// </summary>
        /// <param name="akeyRole">Role ou Client ou Fournisseur ou Commande ou DétailCommande ou Livraison </param>
        /// <returns></returns>
        public async Task<Site> SiteDeRole(AKeyBase akeyRole)
        {
            Role role = await _context.Role.Where(r => r.Uid == akeyRole.KeyParam.Uid && r.Rno == akeyRole.KeyParam.Rno).FirstOrDefaultAsync();
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
            if (timeSpan.TotalDays > TypeEtatRole.JoursInactifAvantExclu())
            {
                return true;
            }
            return false;
        }

        IQueryable<Role> Complète(IQueryable<Role> données)
        {
            return données.Include(d => d.Site);
        }

        public DateTime ChangeEtatSansSauver(Role role, string état)
        {
            role.Etat = état;
            _context.Role.Update(role);
            DateTime date = DateTime.Now;
            ArchiveRole etatRole = new ArchiveRole
            {
                Date = date,
                Etat = état
            };
            etatRole.CopieKey(role);
            _context.ArchiveRole.Add(etatRole);
            return date;
        }

        public async Task<RetourDeService<RoleEtat>> ChangeEtat(Role role, string état)
        {
            RoleEtat roleEtat = RoleEtat.DeDate(ChangeEtatSansSauver(role, état));
            return await SaveChangesAsync(roleEtat);
        }

        public override Role CréeDonnée()
        {
            return new Role();
        }

        public override RoleVue CréeVue(Role donnée)
        {
            RoleVue vue = new RoleVue
            {
                SiteUid = donnée.SiteUid,
                SiteRno = donnée.SiteRno,
                Url = donnée.Site.Url,
                Nom = donnée.Nom,
                Adresse = donnée.Adresse,
                Ville = donnée.Ville,
                FormatNomFichierCommande = donnée.FormatNomFichierCommande,
                FormatNomFichierLivraison = donnée.FormatNomFichierLivraison,
                FormatNomFichierFacture = donnée.FormatNomFichierFacture,
            };
            vue.CopieKey(donnée);
            return vue;
        }

        protected override void CopieVueDansDonnée(RoleVue de, Role vers)
        {
            if (de.Nom != null)
            {
                vers.Nom = de.Nom;
            }
            if (de.Adresse != null)
            {
                vers.Adresse = de.Adresse;
            }
            if (de.Ville != null)
            {
                vers.Ville = de.Ville;
            }
            vers.FormatNomFichierCommande = de.FormatNomFichierCommande;
            vers.FormatNomFichierLivraison = de.FormatNomFichierLivraison;
            vers.FormatNomFichierFacture = de.FormatNomFichierFacture;
        }

        protected override void CopieVuePartielleDansDonnée(RoleVue de, Role vers, Role pourComplèter)
        {
            vers.Nom = de.Nom ?? pourComplèter.Nom;
            vers.Adresse = de.Adresse ?? pourComplèter.Adresse;
            vers.Ville = de.Ville ?? pourComplèter.Ville;
            vers.FormatNomFichierCommande = de.FormatNomFichierCommande ?? pourComplèter.FormatNomFichierCommande;
            vers.FormatNomFichierLivraison = de.FormatNomFichierLivraison ?? pourComplèter.FormatNomFichierLivraison;
            vers.FormatNomFichierFacture = de.FormatNomFichierFacture ?? pourComplèter.FormatNomFichierFacture;
        }

    }
}