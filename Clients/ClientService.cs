using KalosfideAPI.Data;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using KalosfideAPI.Sécurité;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    class GèreArchive : AvecIdUintGèreArchive<Client, ClientAEditer, ArchiveClient>
    {
        public GèreArchive(DbSet<ArchiveClient> dbSetArchive) : base(dbSetArchive)
        {
        }

        protected override ArchiveClient CréeArchive()
        {
            return new ArchiveClient();
        }

        protected override void CopieDonnéeDansArchive(Client donnée, ArchiveClient archive)
        {
            Client.CopieData(donnée, archive);
        }

        protected override ArchiveClient CréeArchiveDesDifférences(Client donnée, ClientAEditer vue)
        {
            ArchiveClient archive = new ArchiveClient
            {
                Id = donnée.Id,
                Date = DateTime.Now
            };
            bool modifié = Client.CopieDifférences(donnée, vue, archive);
            return modifié ? archive : null;
        }
    }

    public class ClientService : AvecIdUintService<Client, ClientAAjouter, ClientAEditer>, IClientService
    {
        private readonly IEnvoieEmailService _emailService;

        public ClientService(ApplicationContext context, IEnvoieEmailService emailService) : base(context)
        {
            _dbSet = _context.Client;
            _gèreArchive = new GèreArchive(_context.ArchiveClient);
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
            dValideSupprime = ValideSupprime;
            _emailService = emailService;
        }

        public async Task<bool> NomPris(string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom).AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(uint id, string nom)
        {
            return await _dbSet.Where(c => c.Nom == nom && c.Id != id).AnyAsync();
        }

        private async Task ValideAjoute(Client donnée, ModelStateDictionary modelState)
        {
            if (await NomPris(donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
        }

        private async Task ValideEdite(Client donnée, ModelStateDictionary modelState)
        {
            if (await NomPrisParAutre(donnée.Id, donnée.Nom))
            {
                ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
            }
        }

        private async Task ValideSupprime(Client donnée, ModelStateDictionary modelState)
        {
            bool avecDocs = await _context.Docs
                .Where(doc => donnée.Id == doc.Id)
                .AnyAsync();
            if (avecDocs)
            {
                ErreurDeModel.AjouteAModelState(modelState, "nonVide");
            }
        }

        public override Client CréeDonnée()
        {
            return new Client();
        }

        protected override void CopieAjoutDansDonnée(ClientAAjouter de, Client vers)
        {
            vers.SiteId = de.SiteId;
            Client.CopieData(de, vers);
            vers.Etat = EtatRole.Nouveau;
        }

        protected override void CopieEditeDansDonnée(ClientAEditer de, Client vers)
        {
            Client.CopieDataSiPasNull(de, vers);
        }

        protected override void CopieVuePartielleDansDonnée(ClientAEditer de, Client vers, Client pourCompléter)
        {
            Client.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
        }

        /// <summary>
        /// Retourne l'email de l'utilsateur si le client gère son compte
        /// </summary>
        /// <param name="idClient">objet ayant la clé du client</param>
        /// <returns>null si le client ne gère pas son compte</returns>
        public async Task<string> Email(uint idClient)
        {
            Utilisateur utilisateur = await _context.Client
                .Where(c => c.Id == idClient)
                .Include(c => c.Utilisateur)
                .Select(c => c.Utilisateur)
                .FirstOrDefaultAsync();
            return utilisateur?.Email;
        }

        private async Task<ClientEtatVue> ClientEtatVue(Client client, long maintenant, int joursInactifAvantExclu)
        {
            ArchiveClient archive = client.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date).Last();
            ClientEtatVue vue = new ClientEtatVue
            {
                Id = client.Id,
                    Date0 = client.Archives.First().Date,
                    Email = client.Utilisateur?.Email,
            };
            Client.CopieData(client, vue);
            if (archive.Etat == EtatRole.Inactif)
            {
                TimeSpan timeSpan = new TimeSpan(maintenant - archive.Date.Ticks);
                if (timeSpan.TotalDays > joursInactifAvantExclu)
                {
                    // changer l'état
                    await ChangeEtat(client, EtatRole.Fermé);
                    vue.Etat = EtatRole.Fermé;
                    vue.DateEtat = DateTime.Now;
                }
            }
            else
            {
                vue.Etat = client.Etat;
                vue.DateEtat = archive.Date;
                if (archive.Etat == EtatRole.Actif)
                {
                    bool avecDocuments = await _context.Docs
                        .Where(d => d.Id == archive.Id)
                        .Include(d => d.Lignes)
                        .Where(d => d.Lignes.Any())
                        .AnyAsync();
                    vue.AvecDocuments = avecDocuments;
                }
            }
            return vue;
        }

        /// <summary>
        /// Retourne la liste des vues contenant les donnéees d'état des clients d'un site ayant un Etat permis
        /// et n'ayant pas d'Utilisateur ou ayant un Utilisateur d'Etat permis.
        /// </summary>
        /// <param name="idSite">Id du site</param>
        /// <param name="étatsClientPermis">Array des EtatClient permis</param>
        /// <param name="étatsUtilisateurPermis">Array des EtatUtilisateur permis</param>
        /// <returns></returns>
        public async Task<List<ClientEtatVue>> ClientsDuSite(uint idSite, EtatRole[] étatsClientPermis, EtatUtilisateur[] étatsUtilisateurPermis)
        {
            List<Client> clients = await _context.Client
                .Where(client => client.SiteId == idSite)
                .Include(client => client.Archives)
                .Include(client => client.Utilisateur)
                .ToListAsync();
            List<ClientEtatVue> vues = new List<ClientEtatVue>();
            long maintenant = DateTime.Now.Ticks;
            int joursInactifAvantExclu = Client.JoursInactifAvantExclu();
            foreach (Client client in clients)
            {
                ClientEtatVue vue = await ClientEtatVue(client, maintenant, joursInactifAvantExclu);
                if ((client.Utilisateur == null || étatsUtilisateurPermis.Contains(client.Utilisateur.Etat))
                    && étatsClientPermis.Contains(client.Etat))
                {
                    vues.Add(vue);
                }
            }
            return vues;
        }

        /// <summary>
        /// Lit dans le bdd un Client avec Site et Utilisateur.
        /// </summary>
        /// <param name="idClient">Id du Client</param>
        /// <returns></returns>
        public async Task<Client> LitClient(uint idClient)
        {
            return await _context.Client
                .Where(c => c.Id == idClient)
                .Include(c => c.Site)
                .Include(c => c.Utilisateur)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne le client du site ayant le nom
        /// </summary>
        /// <param name="idSite"></param>
        /// <param name="nom"></param>
        /// <returns></returns>
        public async Task<Client> ClientDeNom(uint idSite, string nom)
        {
            return await _context.Client
                .Where(c => c.SiteId == idSite)
                .Where(c => c.Nom == nom)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cherche un Client d'un site à partir de l'Email de son Utilisateur.
        /// </summary>
        /// <param name="idSite">Id d'un Site</param>
        /// <param name="email">adresse email</param>
        /// <returns>le Client trouvé, s'il y en a un; null, sinon.</returns>
        public async Task<Client> ClientDeEmail(uint idSite, string email)
        {
            return await _context.Client
                .Include(c => c.Utilisateur)
                .Where(c => c.SiteId == idSite && c.Utilisateur.Email == email)
                .FirstOrDefaultAsync();
        }

        #region Invitation

        /// <summary>
        /// Cherche dans la bdd l'Invitation à partir de son IInvitationKey (Id du Fournisseur, Email de l'invité).
        /// </summary>
        /// <param name="invitationKey">IInvitationKey (Id du Fournisseur, Email de l'invité)</param>
        /// <returns>l'Invitation trouvée si elle existe; null sinon</returns>
        public async Task<Invitation> LitInvitation(IInvitationKey invitationKey)
        {
            Invitation invitation = await _context.Invitation
                .Where(i => i.Id == invitationKey.Id && i.Email == invitationKey.Email)
                .FirstOrDefaultAsync();
            return invitation;
        }

        /// <summary>
        /// Vérifie qu'il y a dans la bdd une Invitation identique à une invitation transmise par l'UI.
        /// </summary>
        /// <param name="invitation">Invitation transmise par l'UI</param>
        /// <returns>l'invitation trouvée avec son Fournisseur incluant son Site et éventuellement le Client à prendre en charge
        /// si elle existe; null sinon</returns>
        public async Task<Invitation> InvitationEnregistrée(Invitation invitation)
        {
            Invitation enregistrée = await _context.Invitation
                .Where(i => i.Id == invitation.Id && i.Email == invitation.Email)
                .Include(i => i.Fournisseur).ThenInclude(f => f.Site)
                .Include(i => i.Client)
                .FirstOrDefaultAsync();
            if (enregistrée == null || enregistrée.Date != invitation.Date || enregistrée.ClientId != invitation.ClientId)
            {
                return null;
            }
            return enregistrée;
        }

        /// <summary>
        /// Cherche dans la bdd une Invitation à prendre en charge un client.
        /// </summary>
        /// <param name="idClient">Id du Client recherché</param>
        /// <returns>l'invitation trouvée avec le Client à prendre en charge, si elle existe; null, sinon.</returns>
        public async Task<Invitation> InvitationDeClientId(uint idClient)
        {
            return await _context.Invitation
                .Where(i => idClient == i.ClientId)
                .FirstOrDefaultAsync(); // il y en a au plus une
        }

        /// <summary>
        /// Envoie un message à l'Email de l'invitation avec un lien contenant l'Invitation encodée.
        /// </summary>
        /// <param name="invitation">Invitation à envoyer</param>
        /// <param name="fournisseur">Fournisseur qui envoie l'Invitation</param>
        /// <param name="client">Client existant éventuel à prendre en charge par l'invité</param>
        /// <returns></returns>
        public async Task EnvoieEmailInvitation(Invitation invitation, Fournisseur fournisseur, Client client)
        {
            string objet = "Devenir client du site " + fournisseur.Site.Titre;
            string urlBase = ClientApp.Url(ClientApp.DevenirClient);
            string message = client == null
                ? "Vous pouvez créer votre compte client de " + fournisseur.Site.Titre
                : "";

            await _emailService.EnvoieEmail<Invitation>(invitation.Email, objet, message, urlBase, invitation, null);
        }

        /// <summary>
        /// Enregistre dans la bdd l'envoi d'une Invitation.
        /// </summary>
        /// <param name="invitation">Invitation envoyée</param>
        /// <param name="enregistrée">enregistrement dans la bdd d'une Invitation précédente envoyée par le même Fournisseur au même Email</param>
        /// <returns></returns>
        public async Task<RetourDeService> EnregistreInvitation(Invitation invitation, Invitation enregistrée)
        {
            if (enregistrée != null)
            {
                // il y a déjà un enregistrement d'une Invitation envoyée par le même Fournisseur au même Email
                // on met à jour cette Invitation
                enregistrée.Date = invitation.Date;
                enregistrée.ClientId = invitation.ClientId;
                _context.Invitation.Update(enregistrée);
            }
            else
            {
                // c'est la première  Invitation envoyée par ce Fournisseur à cet Email
                // on ajoute cette Invitation
                _context.Invitation.Add(invitation);
            }
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Supprime une Invitation de la bdd.
        /// </summary>
        /// <param name="invitation">Invitation à supprimer</param>
        /// <returns></returns>
        public async Task<RetourDeService> SupprimeInvitation(Invitation invitation)
        {
            _context.Invitation.Remove(invitation);
            return await SaveChangesAsync();
        }


        /// <summary>
        /// Invitation contenue dans le code du lien envoyé dans le message email d'invitation.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>l'Invitation contenue dans le code, si le code est valide; null, sinon</returns>
        public Invitation DécodeInvitation(string code)
        {
            return _emailService.DécodeCodeDeEmail<Invitation>(code);
        }

        /// <summary>
        /// Liste des Invitations d'un Site sans leur Id qui est celle du Fournisseur
        /// </summary>
        /// <param name="idSite"></param>
        /// <returns></returns>
        public async Task<List<InvitationVue>> InvitationsSansId(uint idSite)
        {
            List<Invitation> invitations = await _context.Invitation
                .Where(i => i.Id == idSite)
                .ToListAsync();
            return invitations
                .Select(i => new InvitationVue(i))
                .ToList();
        }

        #endregion

        /// <summary>
        /// Crée un nouveau Client.
        /// </summary>
        /// <param name="idSite">Id du Site</param>
        /// <param name="idUtilisateur">Id de l'Utilisateur</param>
        /// <param name="vue">Données du Client à créer</param>
        /// <returns></returns>
        public async Task<RetourDeService> CréeClient(uint idSite, string idUtilisateur, IClientData vue)
        {
            // on crée le Client
            Client client = new Client
            {
                UtilisateurId = idUtilisateur,
                SiteId = idSite,
                Etat = EtatRole.Nouveau
            };
            Client.CopieData(vue, client);

            return await Ajoute(client);
        }

        /// <summary>
        /// Crée un nouveau Client et si il y a un ancien Client attribue ses archives et ses documents au client créé.
        /// </summary>
        /// <param name="idSite">Id du Site</param>
        /// <param name="idUtilisateur">Id de l'Utilisateur</param>
        /// <param name="vue">Données du Client à créer</param>
        /// <param name="clientInvité">Client créé par le fournisseur que le nouveau Client va prendre en charge</param>
        /// <returns></returns>
        public async Task<RetourDeService> CréeClient(uint idSite, string idUtilisateur, IClientData vue, Client clientInvité)
        {
            // on crée le Client
            Client client = new Client
            {
                UtilisateurId = idUtilisateur,
                SiteId = idSite,
                Etat = EtatRole.Nouveau
            };
            Client.CopieData(vue, client);
            if (clientInvité == null)
            {
                // il n'y a ni archives ni documents à récupérer
                // on ajoute aux tables Client et ArchiveClient
                return await Ajoute(client);
            }

            // il y a des archives et peut-être des documents à réattribuer
            // on ajoute seulement à la table Client (sans utiliser Ajoute)
            _context.Client.Add(client);
            RetourDeService<Client> retour = await SaveChangesAsync(client);
            if (!retour.Ok)
            {
                return retour;
            }

            // on doit attribuer au client créé les archives du Client existant
            List<ArchiveClient> archives = await _context.ArchiveClient
                .Where(a => a.Id == clientInvité.Id)
                .ToListAsync();
            archives = archives
                .Select(a => ArchiveClient.Clone(client.Id, a))
                .ToList();
            // et ajouter une archive enregistrant le passage à l'état Nouveau
            ArchiveClient archive = new ArchiveClient
            {
                Id = client.Id,
                Etat = EtatRole.Nouveau,
                Date = DateTime.Now
            };
            archives.Add(archive);
            _context.ArchiveClient.AddRange(archives);

            // on doit attribuer au client créé les documents et les lignes du client existant et supprimer celui-ci
            List<DocCLF> anciensDocs = await _context.Docs
                .Where(d => d.Id == clientInvité.Id)
                .ToListAsync();
            // s'il n'y a pas de documents, il n'y a rien à réattribuer
            if (anciensDocs.Count != 0)
            {
                List<LigneCLF> anciennesLignes = await _context.Lignes
                    .Where(l => l.Id == clientInvité.Id)
                    .ToListAsync();
                // s'il n'y a pas de lignes, il n'y a rien à réattribuer
                if (anciennesLignes.Count != 0)
                {
                    List<DocCLF> nouveauxDocs = anciensDocs
                        .Select(d => DocCLF.Clone(client.Id, d))
                        .ToList();
                    List<LigneCLF> nouvellesLignes = anciennesLignes
                        .Select(l => LigneCLF.Clone(client.Id, l))
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

            // supprime l'ancien client et en cascade ses archives, ses documents et ses lignes
            SupprimeSansSauver(clientInvité);
            _context.Client.Remove(clientInvité);
            await SaveChangesAsync();
            return retour;
        }

        public DateTime ChangeEtatSansSauver(Client client, EtatRole état)
        {
            client.Etat = état;
            _context.Client.Update(client);
            DateTime date = DateTime.Now;
            ArchiveClient archive = new ArchiveClient
            {
                Id = client.Id,
                Date = date,
                Etat = état
            };
            _context.ArchiveClient.Add(archive);
            return date;
        }

        private async Task<RetourDeService<ClientEtatVue>> ChangeEtat(Client client, EtatRole état)
        {
            DateTime date = ChangeEtatSansSauver(client, état);
            ClientEtatVue vue = new ClientEtatVue
            {
                Id = client.Id,
                DateEtat = date
            };
            return await SaveChangesAsync(vue);
        }

        /// <summary>
        /// Change l'Etat du Client en Actif et sauvegarde
        /// </summary>
        /// <param name="clientNonActif"></param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état</returns>
        public async Task<RetourDeService<ClientEtatVue>> Active(Client clientNonActif)
        {
            return await ChangeEtat(clientNonActif, EtatRole.Actif);
        }

        /// <summary>
        /// Supprime toutes les modifications apportées à la bdd depuis et y compris la création du Client sur Invitation
        /// </summary>
        /// <param name="clientNouveau">Client qui a été créé en répondant à une Invitation</param>
        /// <returns>RetourDeService  d'un ClientEtatVue contenant un Client identique à celui que l'Invitation invitait à gérer s'il y en avait un, null sinon</returns>
        public new async Task<RetourDeService<ClientEtatVue>> Supprime(Client clientNouveau)
        {
            Client rétabli = null;
            ClientEtatVue vue = null;
            List<ArchiveClient> archives = await _context.ArchiveClient
                .Where(a => a.Id == clientNouveau.Id)
                .OrderBy(a => a.Date)
                .ToListAsync();
            // index de l'archive ayant enregiistré le
            int indexCréation = archives.FindIndex(a => a.Etat == EtatRole.Nouveau);
            if (indexCréation != 0)
            {
                // le compte existait avant le rattachement au client, il faut le rétablir
                rétabli = new Client
                {
                    Id = clientNouveau.Id,
                    SiteId = clientNouveau.SiteId
                };
                // date du dernier changement d'état à fixer à partir des archives
                DateTime dateEtat = archives.ElementAt(0).Date;

                // Fixe les champs du Role à rétablir avec les champs non nuls de l'archive
                // Si l'archive a un Etat fixe la date de changement d'état
                ArchiveClient rétablitArchive(ArchiveClient archive)
                {
                    Client.CopieDataSiPasNull(archive, rétabli);
                    ArchiveClient archiveRétablie = new ArchiveClient
                    {
                        Id = clientNouveau.Id
                    };
                    Client.CopieData(archive, archiveRétablie);
                    archiveRétablie.Date = archive.Date;
                    if (archive.Etat != null)
                    {
                        rétabli.Etat = archive.Etat.Value;
                        dateEtat = archive.Date;
                    }
                    return archiveRétablie;
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
                List<ArchiveClient> archivesRétablies = archives
                    .GetRange(0, indexCréation)
                    .Select(a => rétablitArchive(a))
                    .ToList();


                // on ajoute seulement à la table Role
                _context.Client.Add(rétabli);
                // il faut sauvegarder pour pouvoir ajouter les élément dépendants
                RetourDeService retour = await SaveChangesAsync();
                if (!retour.Ok)
                {
                    return transformeErreur(retour);
                }
                vue = new ClientEtatVue
                {
                    Id = rétabli.Id,
                    Etat = EtatRole.Actif,
                    DateEtat = dateEtat
                };
                Client.CopieData(rétabli, vue);

                // on ajoute les archives
                _context.ArchiveClient.AddRange(archivesRétablies);

                // date du passage à l'état nouveau
                DateTime dateCréation = archives.ElementAt(indexCréation).Date;
                // on doit attribuer au client créé les documents et les lignes du client créés avant le passage à l'état nouveau
                List<DocCLF> anciensDocs = await _context.Docs
                    .Where(d => d.Id == clientNouveau.Id && d.Date < dateCréation)
                    .ToListAsync();
                // s'il n'y a pas de documents, il n'y a rien à réattribuer
                if (anciensDocs.Count != 0)
                {
                    List<LigneCLF> anciennesLignes = await _context.Lignes
                        .Where(l => l.Id == clientNouveau.Id && anciensDocs.Where(d => d.No == l.No).Any())
                        .ToListAsync();
                    // s'il n'y a pas de lignes, il n'y a rien à réattribuer
                    if (anciennesLignes.Count != 0)
                    {
                        vue.AvecDocuments = true;
                        List<DocCLF> nouveauxDocs = anciensDocs
                            .Select(d => DocCLF.Clone(rétabli.Id, d))
                            .ToList();
                        List<LigneCLF> nouvellesLignes = anciennesLignes
                            .Select(l => LigneCLF.Clone(rétabli.Id, l))
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
            _context.Client.Remove(clientNouveau);
            return await SaveChangesAsync(vue);
        }

        /// <summary>
        /// Si le Client a été créé par le fournisseur et s'il y a des documents avec des lignes, change son Etat en Fermé  il est supprimé sinon.
        /// Si le Client a été créé par le fournisseur et est vide, supprime le Client.
        /// Si le Client a été créé en répondant à une invitation, change son Etat en Inactif et il passera automatiquement à l'Etat Fermé
        /// quand le client se connectera ou quand le fournisseur chargera la liste des clients aprés 60 jours.
        /// </summary>
        /// <param name="clientActif"></param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état ou null si le Client a été supprimé</returns>
        public async Task<RetourDeService<ClientEtatVue>> Inactive(Client clientActif)
        {
            if (clientActif.UtilisateurId != null)
            {
                // le compte est géré par le client, il faut le désactiver
                // il sera automatiquement fermé aprés 60 jours
                return await ChangeEtat(clientActif, EtatRole.Inactif);
            }
            // le compte est géré par le fournisseur, il faut le fermer s'il y a des documents avec des lignes, le supprimer sinon
            bool avecLignes = await _context.Lignes
                .Where(l => l.Id == clientActif.Id)
                .AnyAsync();
            if (avecLignes)
            {
                return await ChangeEtat(clientActif, EtatRole.Fermé);
            }
            else
            {
                _context.Client.Remove(clientActif);
                return await SaveChangesAsync<ClientEtatVue>(null);
            }
        }

    }
}
