using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Roles;
using KalosfideAPI.Utilisateurs;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    class GèreArchive : Partages.KeyParams.GéreArchive<Client, ClientVue, ArchiveClient>
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
        private readonly IUtilisateurService _utilisateurService;
        private readonly IRoleService _roleService;
        public ClientService(ApplicationContext context,
            IUtilisateurService utilisateurService, IRoleService roleService) : base(context)
        {
            _dbSet = _context.Client;
            _utilisateurService = utilisateurService;
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

        /// <summary>
        /// retourne le plus grand des numéros d'anonymat utilisés dans toute la base de données
        /// </summary>
        /// <returns></returns>
        private async Task<int> MaxNoAnonymat()
        {
            List<Client> anonymes = await _context.Client
                .Where(c => NomsRéservés.EstCommeAnonyme(c.Nom))
                .ToListAsync();
            return anonymes.Count() == 0 ? 0 : anonymes.Select(c => NomsRéservés.NoAnonyme(c.Nom)).Max();
        }

        /// <summary>
        /// remplace les données personnelles du client par des valeurs anonymes
        /// </summary>
        /// <param name="cru">Client Role Utilisateur du client</param>
        /// <param name="noAnonymat">numéro d'anonymat</param>
        /// <returns></returns>
        private async Task<CRU> RendAnonyme(CRU cru, int noAnonymat)
        {
            // il faut anonymer le client
            cru.Client.Nom = NomsRéservés.NomAnonyme(noAnonymat);
            cru.Client.Adresse = NomsRéservés.AdresseAnonyme;
            _context.Client.Update(cru.Client);

            // if faut anonymer le journal des modifications du client
            // anonymer l'archive initiale
            ArchiveClient archive = await _context.ArchiveClient
                .Where(c => cru.Client.AMêmeKey(c))
                .FirstAsync();
            archive.Nom = cru.Client.Nom;
            archive.Adresse = cru.Client.Adresse;
            _context.ArchiveClient.Update(archive);
            // supprimer les archives suivantes
            ArchiveClient[] archives = await _context.ArchiveClient
                .Where(c => cru.Client.AMêmeKey(c))
                .Skip(1)
                .ToArrayAsync();
            _context.ArchiveClient.RemoveRange(archives);

            await _context.SaveChangesAsync();

            // changer l'état
            await _roleService.ChangeEtat(cru.Role, TypeEtatRole.Exclu);
            cru.Role.Etat = TypeEtatRole.Exclu;
            cru.DateEtat = DateTime.Now;

            // supprimer ApplicationUser si aucun role d'état nouveau ou actif ???

            return cru;
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
                AvecCompte = cru.Utilisateur.UserId != null,
                AvecCommandes = cru.AvecCommandes
            };
        }

        private async Task<CRU> FixeDate(CRU cru, ArchiveRole archive, int noAnonymat)
        {
            if (archive.Etat == TypeEtatRole.Inactif)
            {
                if (_roleService.TempsInactifEcoulé(archive.Date))
                {
                    cru = await RendAnonyme(cru, noAnonymat++);
                }
            }
            else
            {
                cru.DateEtat = archive.Date;
                if (archive.Etat == TypeEtatRole.Nouveau || archive.Etat == TypeEtatRole.Actif)
                {
                    bool aCommandé = await _context.Docs
                        .Where(c => c.Uid == archive.Uid && c.Rno == archive.Rno)
                        .AnyAsync();
                    if (aCommandé)
                    {
                        // pour exclure les commandes vides
                        aCommandé = await _context.Lignes
                            .Where(c => c.Uid == archive.Uid && c.Rno == archive.Rno)
                            .AnyAsync();
                    }
                    cru.AvecCommandes = aCommandé;
                }
            }
            return cru;
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
            var x = crus.GroupJoin(_context.ArchiveRole,
                cru => new { cru.Client.Uid, cru.Client.Rno },
                a => new { a.Uid, a.Rno },
                (cru, archives) => new { cru, archive = archives.Last() });
            int noAnonymat = (await MaxNoAnonymat()) + 1;
            crus = (await Task.WhenAll(x.Select(crua => FixeDate(crua.cru, crua.archive, noAnonymat)))).ToList();

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
                .Select(cru => ClientEtatVue(cru))
                .ToList();
        }

        /// <summary>
        /// retourne la liste des vues contenant les donnéees d'état des clients qui ont créé leur compte depuis la date
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public async Task<List<ClientEtatVue>> ClientsDuSite(AKeyUidRno aKeySite, DateTime date)
        {
            return await IQCRUsDuSite(aKeySite)
                .Where(cru => cru.Utilisateur.UserId != null) // avec compte
                .GroupJoin(_context.ArchiveRole,
                cru => new { cru.Client.Uid, cru.Client.Rno },
                a => new { a.Uid, a.Rno },
                (cru, archives) => new { cru, créé = archives.First().Date })
                .Where(cruc => cruc.créé > date)
                .Select(cruc => ClientEtatVue(cruc.cru))
                .ToListAsync();
        }

        /// <summary>
        /// Retourne un IQueryable pouvant produire la liste des clients avce compte du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public IQueryable<Client> IQClientsAvecCompte(AKeyUidRno aKeySite)
        {
            return IQCRUsDuSite(aKeySite)
                .Where(cru => cru.Utilisateur.UserId != null)
                .Where(cru => cru.Role.Etat == TypeEtatRole.Nouveau || cru.Role.Etat == TypeEtatRole.Actif)
                .Select(cru => cru.Client);
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

        private async Task<bool> EstNomFournisseur(AKeyUidRno akeySite, string nom)
        {
            return await _context.Fournisseur
                .Where(f => f.AMêmeKey(akeySite) && f.Nom == nom)
                .AnyAsync();
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
            return await EstNomFournisseur(akeySite, nom)
                || await IQueryNomPrisParUid(akeySite, nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(AKeyUidRno akeySite, AKeyUidRno akey, string nom)
        {
            return await EstNomFournisseur(akeySite, nom)
                || await IQueryNomPrisParUid(akeySite, nom).Where(uid => uid != akey.Uid).AnyAsync();
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

        public async Task<RetourDeService<Client>> Ajoute(AKeyUidRno keySite, IClient vue)
        {
            RetourDeService<Utilisateur> retour1 = await _utilisateurService.CréeUtilisateur();
            if (!retour1.Ok)
            {
                return new RetourDeService<Client>(retour1.Type);
            }
            Utilisateur utilisateur = retour1.Entité;
            Role role = new Role
            {
                Uid = utilisateur.Uid,
                Rno = 1,
                Etat = TypeEtatRole.Actif,
                SiteUid = keySite.Uid,
                SiteRno = keySite.Rno
            };
            RetourDeService<Role> retour2 = await _roleService.Ajoute(role);
            if (!retour2.Ok)
            {
                await _utilisateurService.Supprime(utilisateur);
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
                await _utilisateurService.Supprime(utilisateur);
                await _roleService.Supprime(role);
            }
            return retour;
        }

        public async Task<RetourDeService<Client>> Ajoute(ClientVueAjoute vue)
        {
            return await Ajoute(vue, vue);
        }

        public Client CréeClient(Role role, EnregistrementClientVue clientVue)
        {
            Client client = new Client
            {
                Uid = role.Uid,
                Rno = role.Rno,
                Nom = clientVue.Nom,
                Adresse = clientVue.Adresse,
            };
            role.SiteUid = clientVue.SiteUid;
            role.SiteRno = clientVue.SiteRno;
            return client;
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
