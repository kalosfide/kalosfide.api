using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public class ClientService : RoleService, IClientService
    {
        public ClientService(ApplicationContext context) : base(context)
        {
        }

        /// <summary>
        /// Retourne l'email de l'utilsateur si le client gère son compte
        /// </summary>
        /// <param name="aKeyClient">objet ayant la clé du client</param>
        /// <returns>null si le client ne gère pas son compte</returns>
        public async Task<string> Email(AKeyUidRno aKeyClient)
        {
            Utilisateur utilisateur = await _context.Utilisateur
                .Where(u => u.Uid == aKeyClient.Uid)
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync();
            return utilisateur.ApplicationUser?.Email;
        }

        /// <summary>
        /// retourne la liste des RUs des clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        private IQueryable<RU> IQRUsDuSite(AKeyUidRno aKeySite)
        {
            return _context.Role
                .Where(r => r.SiteUid == aKeySite.Uid && r.SiteRno == aKeySite.Rno) // usagers
                .Where(r => r.Uid != aKeySite.Uid || r.Rno != aKeySite.Rno) // sauf fournisseur
                .Include(r => r.Archives)
                .Include(r => r.Utilisateur)
                .Select(r => new RU
                    {
                        Role = r,
                    });
        }

        /// <summary>
        /// retourne la liste des RUs des clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        private async Task<List<RU>> RUsDuSite(AKeyUidRno aKeySite)
        {

            List<RU> crus = await IQRUsDuSite(aKeySite).ToListAsync();
            long maintenant = DateTime.Now.Ticks;
            int joursInactifAvantExclu = TypeEtatRole.JoursInactifAvantExclu();
            foreach (RU cru in crus)
            {
                ArchiveRole archive = cru.Role.Archives.OrderBy(a => a.Date).Last();
                if (archive.Etat == TypeEtatRole.Inactif)
                {
                    TimeSpan timeSpan = new TimeSpan(maintenant - archive.Date.Ticks);
                    if (timeSpan.TotalDays > joursInactifAvantExclu)
                    {
                        // changer l'état
                        await ChangeEtat(cru.Role, TypeEtatRole.Fermé);
                        cru.Role.Etat = TypeEtatRole.Fermé;
                        cru.DateEtat = DateTime.Now;
                    }
                }
                else
                {
                    cru.DateEtat = archive.Date;
                    if (archive.Etat == TypeEtatRole.Actif)
                    {
                        bool avecDocuments = await _context.Docs
                            .Where(d => d.Uid == archive.Uid && d.Rno == archive.Rno)
                            .Include(d => d.Lignes)
                            .Where(d => d.Lignes.Any())
                            .AnyAsync();
                        cru.AvecDocuments = avecDocuments;
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
            ClientEtatVue ClientEtatVue(RU ru)
            {
                return new ClientEtatVue
                {
                    Uid = ru.Role.Uid,
                    Rno = ru.Role.Rno,
                    Nom = ru.Role.Nom,
                    Adresse = ru.Role.Adresse,
                    Etat = ru.Role.Etat,
                    Date0 = ru.Role.Archives.First().Date,
                    DateEtat = ru.DateEtat,
                    Email = ru.Role.Utilisateur.ApplicationUser?.Email,
                    AvecDocuments = ru.AvecDocuments
                };
            }
            List<RU> rus = await RUsDuSite(aKeySite);
            return rus
                .OrderBy(ru => ru.Role.Nom)
                .Select(ru => ClientEtatVue(ru))
                .ToList();
        }

        /// <summary>
        /// retourne le nombre de clients actifs du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        public async Task<int> NbClients(AKeyUidRno aKeySite)
        {
            List<RU> rus = await RUsDuSite(aKeySite);
            return rus
                .Where(ru => ru.Role.Etat == TypeEtatRole.Actif)
                .Count();
        }

        /// <summary>
        /// Lit dans le bdd un Role avec Site et Utilisateur et éventuellement ApplicationUser
        /// </summary>
        /// <param name="uid">Uid du role à lire</param>
        /// <param name="rno">Rno du role à lire</param>
        /// <returns></returns>
        public async Task<Role> LitRole(string uid, int rno)
        {
            return await _context.Role
                .Where(r => r.Uid == uid && r.Rno == rno)
                .Include(r => r.Site)
                .Include(r => r.Utilisateur).ThenInclude(u => u.ApplicationUser)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne le client du site ayant le nom
        /// </summary>
        /// <param name="akeySite"></param>
        /// <param name="nom"></param>
        /// <returns></returns>
        public async Task<Role> ClientDeNom(AKeyUidRno akeySite, string nom)
        {
            return await _context.Role
                // usager du site
                .Where(r => r.SiteUid == akeySite.Uid && r.SiteRno == akeySite.Rno)
                // pas le fournisseur
                .Where(r => r.Uid != r.SiteUid || r.Rno != r.SiteRno)
                .Where(c => c.Nom == nom)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crée un Role
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        private async Task<Role> CréeRole(Utilisateur utilisateur, AKeyUidRno keySite, IRoleData vue)
        {
            return new Role
            {
                Uid = utilisateur.Uid,
                Rno = await DernierNo(utilisateur.Uid) + 1,
                SiteUid = keySite.Uid,
                SiteRno = keySite.Rno,
                Nom = vue.Nom,
                Adresse = vue.Adresse,
            };
        }

        /// <summary>
        /// Ajoute à la bdd un nouveau Role et l'archive correspondante
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        public async Task<RetourDeService<Role>> Ajoute(Utilisateur utilisateur, AKeyUidRno keySite, IRoleData vue)
        {
            Role role = await CréeRole(utilisateur, keySite, vue);
            role.Etat = TypeEtatRole.Actif;
            RetourDeService<Role> retour = await Ajoute(role);
            return retour;
        }

        /// <summary>
        /// Ajoute à la bdd un nouveau Role et l'archive correspondante
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        public async Task<RetourDeService<Role>> Ajoute(Utilisateur utilisateur, ClientVueAjoute vue)
        {
            return await Ajoute(utilisateur, vue, vue);
        }

        /// <summary>
        /// Crée un nouveau Role de Client et si il y a un ancien Client attribue ses archives et ses documents au role créé
        /// </summary>
        /// <param name="site"></param>
        /// <param name="utilisateur"></param>
        /// <param name="clientInvité"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        public async Task<RetourDeService> CréeRoleClient(Site site, Utilisateur utilisateur, Role clientInvité, IRoleData vue)
        {
            // on crée le Client
            Role role = await CréeRole(utilisateur, site, vue);
            // le Role doit être créé dans l'état Nouveau
            role.Etat = TypeEtatRole.Nouveau;
            if (clientInvité == null)
            {
                // il n'y a ni archives ni documents à récupérer
                // on ajoute aux tables Role et ArchiveRole
                return await Ajoute(role);
            }

            // il y a des archives et peut-être des documents à réattribuer
            // on ajoute seulement à la table Role (sans utiliser Ajoute)
            _context.Role.Add(role);
            RetourDeService<Role> retour = await SaveChangesAsync(role);
            if (!retour.Ok)
            {
                return retour;
            }

            // on doit attribuer au client créé les archives du Role existant
            List<ArchiveRole> archives = await _context.ArchiveRole
                .Where(a => a.Uid == clientInvité.Uid && a.Rno == clientInvité.Rno)
                .ToListAsync();
            archives = archives
                .Select(a => ArchiveRole.Clone(role.Uid, role.Rno, a))
                .ToList();
            // et ajouter une archive enregistrant le passage à l'état Nouveau
            ArchiveRole archive = new ArchiveRole
            {
                Uid = role.Uid,
                Rno = role.Rno,
                Etat = TypeEtatRole.Nouveau,
                Date = DateTime.Now
            };
            archives.Add(archive);
            _context.ArchiveRole.AddRange(archives);

            // on doit attribuer au client créé les documents et les lignes du client existant et supprimer celui-ci
            List<DocCLF> anciensDocs = await _context.Docs
                .Where(d => d.Uid == clientInvité.Uid && d.Rno == clientInvité.Rno)
                .ToListAsync();
            // s'il n'y a pas de documents, il n'y a rien à réattribuer
            if (anciensDocs.Count != 0)
            {
                List<LigneCLF> anciennesLignes = await _context.Lignes
                    .Where(l => l.Uid == clientInvité.Uid && l.Rno == clientInvité.Rno)
                    .ToListAsync();
                // s'il n'y a pas de lignes, il n'y a rien à réattribuer
                if (anciennesLignes.Count != 0)
                {
                    List<DocCLF> nouveauxDocs = anciensDocs
                        .Select(d => DocCLF.Clone(role.Uid, role.Rno, d))
                        .ToList();
                    List<LigneCLF> nouvellesLignes = anciennesLignes
                        .Select(l => LigneCLF.Clone(role.Uid, role.Rno, l))
                        .ToList();
                    _context.Docs.AddRange(nouveauxDocs);
                    var r = await SaveChangesAsync();
                    if (r.Ok)
                    {
                        _context.Lignes.AddRange(nouvellesLignes);
                        r = await SaveChangesAsync();
                    }
                }
            }

            // l'utilisateur d'un Client créé par le fournisseur n'a que ce role et doit être supprimé.

            // supprime l'ancien client et en cascade ses archives, ses documents et ses lignes
            SupprimeSansSauver(clientInvité);
            _context.Role.Remove(clientInvité);
            await SaveChangesAsync();
            return retour;
        }

        private new async Task<RetourDeService<ClientEtatVue>> ChangeEtat(Role role, string état)
        {
            DateTime date = ChangeEtatSansSauver(role, état);
            ClientEtatVue vue = new ClientEtatVue
            {
                Uid = role.Uid,
                Rno = role.Rno,
                DateEtat = date
            };
            return await SaveChangesAsync(vue);
        }

        /// <summary>
        /// Change l'Etat du Role en Actif et sauvegarde
        /// </summary>
        /// <param name="roleNonActif"></param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état</returns>
        public async Task<RetourDeService<ClientEtatVue>> Active(Role roleNonActif)
        {
            return await ChangeEtat(roleNonActif, TypeEtatRole.Actif);
        }

        /// <summary>
        /// Supprime toutes les modifications apportées à la bdd depuis et y compris la création du Role sur Invitation
        /// </summary>
        /// <param name="roleNouveau">Role qui a été créé en répondant à une Invitation</param>
        /// <returns>RetourDeService  d'un ClientEtatVue contenant un Role identique à celui que l'Invitation invitait à gérer s'il y en avait un, null sinon</returns>
        public new async Task<RetourDeService<ClientEtatVue>> Supprime(Role roleNouveau)
        {
            Role rétabli = null;
            ClientEtatVue vue = null;
            List<ArchiveRole> archives = await _context.ArchiveRole
                .Where(a => a.Uid == roleNouveau.Uid && a.Rno == roleNouveau.Rno)
                .OrderBy(a => a.Date)
                .ToListAsync();
            // index de l'archive ayant enregiistré le
            int indexCréation = archives.FindIndex(a => a.Etat == TypeEtatRole.Nouveau);
            if (indexCréation != 0)
            {
                // le compte existait avant le rattachement au client, il faut le rétablir
                rétabli = await CréeRole(roleNouveau.Utilisateur, roleNouveau.Site, roleNouveau);
                // date du dernier changement d'état à fixer à partir des archives
                DateTime dateEtat = archives.ElementAt(0).Date;

                // Fixe les champs du Role à rétablir avec les champs non nuls de l'archive
                // Si l'archive a un Etat fixe la date de changement d'état
                ArchiveRole rétablitArchive(ArchiveRole archive)
                {
                    if (archive.Nom != null)
                    {
                        rétabli.Nom = archive.Nom;
                    }
                    if (archive.Adresse != null)
                    {
                        rétabli.Adresse = archive.Adresse;
                    }
                    if (archive.Etat != null)
                    {
                        rétabli.Etat = archive.Etat;
                        dateEtat = archive.Date;
                    }
                    if (archive.Ville != null)
                    {
                        rétabli.Ville = archive.Ville;
                    }
                    if (archive.FormatNomFichierCommande != null)
                    {
                        rétabli.FormatNomFichierCommande = archive.FormatNomFichierCommande;
                    }
                    if (archive.FormatNomFichierLivraison != null)
                    {
                        rétabli.FormatNomFichierLivraison = archive.FormatNomFichierLivraison;
                    }
                    if (archive.FormatNomFichierFacture != null)
                    {
                        rétabli.FormatNomFichierFacture = archive.FormatNomFichierFacture;
                    }
                    return ArchiveRole.Clone(rétabli.Uid, rétabli.Rno, archive);
                }

                // transforme un RetourDeService avec erreur en RetourDeService<ClientEtatVue> avec la même erreur
                RetourDeService<ClientEtatVue> transformeErreur(RetourDeService retour)
                {
                    // on copie l'erreur dans RetourDeService de ClientEtatVue
                    RetourDeService<ClientEtatVue> retourVue = new RetourDeService<ClientEtatVue>(retour.Type);
                    if (retour.IdentityError)
                    {
                        retourVue.Objet = retour.Objet;
                    }
                    return retourVue;
                }

                // on doit attribuer au client créé les archives antérieures au passage à l'état nouveau
                // et rétablir ses champs en fonction de ces archives
                List<ArchiveRole> archivesRétablies = archives
                    .GetRange(0, indexCréation)
                    .Select(a => rétablitArchive(a))
                    .ToList();


                // on ajoute seulement à la table Role
                _context.Role.Add(rétabli);
                // il faut sauvegarder pour pouvoir ajouter les élément dépendants
                RetourDeService retour = await SaveChangesAsync();
                if (!retour.Ok)
                {
                    return transformeErreur(retour);
                }
                vue = new ClientEtatVue
                {
                    Uid = rétabli.Uid,
                    Rno = rétabli.Rno,
                    Etat = TypeEtatRole.Actif,
                    DateEtat = dateEtat
                };
                Role.CopieDef(rétabli, vue);

                // on ajoute les archives
                _context.ArchiveRole.AddRange(archivesRétablies);

                // date du passage à l'état nouveau
                DateTime dateCréation = archives.ElementAt(indexCréation).Date;
                // on doit attribuer au client créé les documents et les lignes du client créés avant le passage à l'état nouveau
                List<DocCLF> anciensDocs = await _context.Docs
                    .Where(d => d.Uid == roleNouveau.Uid && d.Rno == roleNouveau.Rno && d.Date < dateCréation)
                    .ToListAsync();
                // s'il n'y a pas de documents, il n'y a rien à réattribuer
                if (anciensDocs.Count != 0)
                {
                    List<LigneCLF> anciennesLignes = await _context.Lignes
                        .Where(l => l.Uid == roleNouveau.Uid && l.Rno == roleNouveau.Rno && anciensDocs.Where(d => d.No == l.No).Any())
                        .ToListAsync();
                    // s'il n'y a pas de lignes, il n'y a rien à réattribuer
                    if (anciennesLignes.Count != 0)
                    {
                        vue.AvecDocuments = true;
                        List<DocCLF> nouveauxDocs = anciensDocs
                            .Select(d => DocCLF.Clone(rétabli.Uid, rétabli.Rno, d))
                            .ToList();
                        List<LigneCLF> nouvellesLignes = anciennesLignes
                            .Select(l => LigneCLF.Clone(rétabli.Uid, rétabli.Rno, l))
                            .ToList();
                        _context.Docs.AddRange(nouveauxDocs);
                        retour = await SaveChangesAsync();
                        if (retour.Ok)
                        {
                            _context.Lignes.AddRange(nouvellesLignes);
                            retour = await SaveChangesAsync();
                        }
                        if (!retour.Ok)
                        {
                            return transformeErreur(retour);
                        }
                    }
                }
            }
            _context.Role.Remove(roleNouveau);
            return await SaveChangesAsync(vue);
        }

        /// <summary>
        /// Si le Role a été créé par le fournisseur et s'il y a des documents avec des lignes, change son Etat en Fermé  il est supprimé sinon.
        /// Si le Role a été créé par le fournisseur et est vide, supprime le Role.
        /// Si le Role a été créé en répondant à une invitation, change son Etat en Inactif et il passera automatiquement à l'Etat Fermé
        /// quand le client se connectera ou quand le fournisseur chargera la liste des clients aprés 60 jours.
        /// </summary>
        /// <param name="roleActif"></param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état ou null si le Role a été supprimé</returns>
        public async Task<RetourDeService<ClientEtatVue>> Inactive(Role roleActif)
        {
            if (roleActif.Utilisateur.UserId != null)
            {
                // le compte est géré par le client, il faut le désactiver
                // il sera automatiquement fermé aprés 60 jours
                return await ChangeEtat(roleActif, TypeEtatRole.Inactif);
            }
            // le compte est géré par le fournisseur, il faut le fermer s'il y a des documents avec des lignes, le supprimer sinon
            bool avecLignes = await _context.Lignes
                .Where(l => l.Uid == roleActif.Uid && l.Rno == roleActif.Rno)
                .AnyAsync();
            if (avecLignes)
            {
                return await ChangeEtat(roleActif, TypeEtatRole.Fermé);
            }
            else
            {
                _context.Role.Remove(roleActif);
                return await SaveChangesAsync<ClientEtatVue>(null);
            }
        }

    }
}
