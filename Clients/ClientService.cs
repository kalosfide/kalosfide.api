using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Roles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    class GèreArchive : Partages.KeyParams.GéreArchiveUidRno<Client, ClientVue, ArchiveClient>
    {
        public GèreArchive(DbSet<Client> dbSet, DbSet<ArchiveClient> dbSetArchive) : base(dbSet, dbSetArchive)
        { }

        protected override ArchiveClient CréeArchive()
        {
            return new ArchiveClient();
        }

        protected override void CopieDonnéeDansArchive(Client donnée, ArchiveClient archive)
        {
            archive.Nom = donnée.Nom;
            archive.Adresse = donnée.Adresse;
        }

        protected override ArchiveClient CréeArchiveDesDifférences(Client donnée, ClientVue vue)
        {
            bool modifié = false;
            ArchiveClient archive = new ArchiveClient();
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                archive.Nom = donnée.Nom;
                donnée.Nom = vue.Nom;
                modifié = true;
            }
            if (vue.Adresse != null && donnée.Adresse != vue.Adresse)
            {
                donnée.Adresse = vue.Adresse;
                archive.Adresse = vue.Adresse;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class ClientService : KeyUidRnoService<Client, ClientVue>, IClientService
    {
        private readonly IRoleService _roleService;
        public ClientService(ApplicationContext context,
            IRoleService roleService) : base(context)
        {
            _dbSet = _context.Client;
            _roleService = roleService;
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveClient);
        }

        /// <summary>
        /// retourne vrai si le client peut se connecter
        /// </summary>
        /// <param name="aKeyClient"></param>
        /// <returns></returns>
        public async Task<bool> AvecCompte(AKeyUidRno aKeyClient)
        {
            string userId = await _context.Utilisateur
                .Where(u => u.Uid == aKeyClient.Uid)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
            return userId != null;
        }

        private ClientEtatVue ClientEtatVue(CRU cru)
        {
            return new ClientEtatVue
            {
                Uid = cru.Role.Uid,
                Rno = cru.Role.Rno,
                Nom = cru.Client.Nom,
                Adresse = cru.Client.Adresse,
                Etat = cru.Role.Etat,
                DateEtat = cru.DateEtat,
                Compte = cru.Utilisateur.UserId == null ? "N" : "O",
                AvecCommandes = cru.AvecCommandes
            };
        }

        /// <summary>
        /// retourne la liste des CRUs des clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        private IQueryable<CRU> IQCRUsDuSite(AKeyUidRno aKeySite)
        {
            return _context.Role
                .Where(r => r.SiteUid == aKeySite.Uid && r.SiteRno == aKeySite.Rno) // usagers
                .Where(r => r.Uid != aKeySite.Uid) // sauf fournisseur
                .Include(r => r.Archives)
                .Join(_context.Utilisateur,
                    r => r.Uid,
                    u => u.Uid,
                    (role, utilisateur) => new { role, utilisateur })
                .Join(_context.Client,
                    ru => new { ru.role.Uid, ru.role.Rno },
                    c => new { c.Uid, c.Rno },
                    (ru, client) => new CRU
                    {
                        Client = client,
                        Role = ru.role,
                        Utilisateur = ru.utilisateur
                    });
        }

        /// <summary>
        /// retourne la liste des CRUs des clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        private async Task<List<CRU>> CRUsDuSite(AKeyUidRno aKeySite)
        {

            List<CRU> crus = await IQCRUsDuSite(aKeySite).ToListAsync();
            long maintenant = DateTime.Now.Ticks;
            int joursInactifAvantExclu = TypeEtatRole.JoursInactifAvantExclu();
            foreach (CRU cru in crus)
            {
                ArchiveRole archive = cru.Role.Archives.OrderBy(a => a.Date).Last();
                if (archive.Etat == TypeEtatRole.Inactif)
                {
                    TimeSpan timeSpan = new TimeSpan(maintenant - archive.Date.Ticks);
                    if (timeSpan.TotalDays > joursInactifAvantExclu)
                    {
                        // changer l'état
                        await _roleService.ChangeEtat(cru.Role, TypeEtatRole.Exclu);
                        cru.Role.Etat = TypeEtatRole.Exclu;
                        cru.DateEtat = DateTime.Now;
                    }
                }
                else
                {
                    cru.DateEtat = archive.Date;
                    if (archive.Etat == TypeEtatRole.Nouveau || archive.Etat == TypeEtatRole.Actif)
                    {
                        bool aCommandé = await _context.Docs
                            .Where(d => d.Uid == archive.Uid && d.Rno == archive.Rno)
                            .Include(d => d.Lignes)
                            .Where(d => d.Lignes.Any())
                            .AnyAsync();
                        cru.AvecCommandes = aCommandé;
                    }
                }
            }

            return crus;
        }

        /// <summary>
        /// retourne la liste des vues contenant les donnéees d'état des clients non exclus du site défini par la clé
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public async Task<List<ClientEtatVue>> ClientsDuSite(AKeyUidRno aKeySite)
        {
            List<CRU> crus = await CRUsDuSite(aKeySite);
            return crus
                .OrderBy(cru => cru.Client.Nom)
                .Select(cru => ClientEtatVue(cru))
                .ToList();
        }

        /// <summary>
        /// Retourne la liste des clients d'état nouveau après avoir fait passer à l'état actif ceux qui ont créé leur compte avant la date
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public async Task<List<ClientEtatVue>> NouveauxClients(AKeyUidRno aKeySite, DateTime date)
        {
            List<CRU> crus = await IQCRUsDuSite(aKeySite)
                .Where(cru => cru.Utilisateur.UserId != null) // avec compte
                .Where(cru => cru.Role.Etat == TypeEtatRole.Nouveau)
                .ToListAsync();
            List<CRU> àActiver = crus
                .Where(cru => cru.DateEtat <= date)
                .ToList();
            àActiver.ForEach(cru => _roleService.ChangeEtatSansSauver(cru.Role, TypeEtatRole.Actif));
            RetourDeService retour = await SaveChangesAsync();
            List<ClientEtatVue> àRetourner = crus
                .Where(cru => cru.DateEtat > date)
                .Select(cru => ClientEtatVue(cru))
                .ToList();
            return àRetourner;
        }

        /// <summary>
        /// retourne le nombre de clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public async Task<int> NbClients(AKeyUidRno aKeySite)
        {
            List<CRU> crus = await CRUsDuSite(aKeySite);
            return crus
                .Where(cru => cru.Role.Etat == TypeEtatRole.Nouveau || cru.Role.Etat == TypeEtatRole.Actif)
                .Count();
        }

        public async Task<KeyParam> KeyParamDuSiteDuClient(KeyParam param)
        {
            return await _context.Role
                .Where(r => r.Uid == param.Uid && r.Rno == param.Rno)
                .Select(r => new KeyParam { Uid = r.SiteUid, Rno = r.SiteRno })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">du client et du role</param>
        /// <returns></returns>
        public async Task<Role> Role(KeyParam param)
        {
            return await _context.Role
                .Where(r => r.Uid == param.Uid && r.Rno == param.Rno)
                .FirstOrDefaultAsync();
        }

        public async Task<RetourDeService<Role>> ChangeEtat(Role role, string état)
        {
            if (état == TypeEtatRole.Exclu)
            {
                état = TypeEtatRole.Inactif;
            }
            _roleService.ChangeEtatSansSauver(role, état);
            return await _roleService.ChangeEtat(role, état);
        }

        private IQueryable<string> IQueryNomPrisParUid(AKeyUidRno akeySite, string nom)
        {
            return _context.Role
                .Where(r => r.SiteUid == akeySite.Uid && r.SiteRno == akeySite.Rno)
                .Join(_context.Client,
                    r => new { r.Uid, r.Rno },
                    c => new { c.Uid, c.Rno },
                    (r, c) => c
                    )
                .Where(c => c.Nom == nom)
                .Select(c => c.Uid);
        }

        public async Task<bool> NomPris(AKeyUidRno akeySite, string nom)
        {
            return await IQueryNomPrisParUid(akeySite, nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(AKeyUidRno akeySite, AKeyUidRno akey, string nom)
        {
            return await IQueryNomPrisParUid(akeySite, nom).Where(uid => uid != akey.Uid).AnyAsync();
        }

        public async Task ValideAjoute(AKeyUidRno akeySite, IClient client, ModelStateDictionary modelState)
        {
            if (await NomPris(akeySite, client.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris");
            }
        }

        public async Task ValideAjoute(ClientVueAjoute vue, ModelStateDictionary modelState)
        {
            await ValideAjoute(vue, vue, modelState);
        }

        public async Task ValideEdite(KeyUidRno keySite, Client donnée, ModelStateDictionary modelState)
        {
            if (await NomPrisParAutre(keySite, donnée, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nomPris");
            }
        }

        public async Task<RetourDeService<Client>> Ajoute(Utilisateur utilisateur, AKeyUidRno keySite, IClient vue)
        {
            Role role = new Role
            {
                Uid = utilisateur.Uid,
                Rno = await _roleService.DernierNo(utilisateur.Uid) + 1,
                Etat = TypeEtatRole.Actif,
                SiteUid = keySite.Uid,
                SiteRno = keySite.Rno
            };
            RetourDeService<Role> retour2 = await _roleService.Ajoute(role);
            if (!retour2.Ok)
            {
                return new RetourDeService<Client>(retour2.Type);
            }
            Client client = new Client
            {
                Uid = utilisateur.Uid,
                Rno = 1,
                Nom = vue.Nom,
                Adresse = vue.Adresse
            };
            RetourDeService<Client> retour = await Ajoute(client);
            if (!retour.Ok)
            {
                await _roleService.Supprime(role);
            }
            return retour;
        }

        public async Task<RetourDeService<Client>> Ajoute(Utilisateur utilisateur, ClientVueAjoute vue)
        {
            return await Ajoute(utilisateur, vue, vue);
        }

        public override void CopieVueDansDonnée(Client donnée, ClientVue vue)
        {
            if (vue.Nom != null)
            {
                donnée.Nom = vue.Nom;
            }
            if (vue.Adresse != null)
            {
                donnée.Adresse = vue.Adresse;
            }
        }

        public override void CopieVuePartielleDansDonnée(Client donnée, ClientVue vue, Client donnéePourComplèter)
        {
            donnée.Nom = vue.Nom ?? donnéePourComplèter.Nom;
            donnée.Adresse = vue.Adresse ?? donnéePourComplèter.Adresse;
        }

        public override Client CréeDonnée()
        {
            return new Client();
        }

        public override ClientVue CréeVue(Client donnée)
        {
            ClientVue vue = new ClientVue
            {
                Nom = donnée.Nom,
                Adresse = donnée.Adresse,
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }

        public async Task<ClientVue> LitVue(KeyUidRno keyClient)
        {
            Client client = await _context.Client
                .Where(c => c.Uid == keyClient.Uid && c.Rno == keyClient.Rno)
                .FirstAsync();
            ClientVue vue = CréeVue(client);
            return vue;
        }
    }
}
