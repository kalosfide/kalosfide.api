using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class CarteRole
    {
        /// <summary>
        /// Id du Fournisseur ou du Client suivant le cas.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Id du Site.
        /// </summary>
        public uint SiteId { get; set; }

        /// <summary>
        /// Date de la dernière utilisation du site
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class CarteUtilisateur
    {
        public Utilisateur Utilisateur { get; set; }

        /// <summary>
        /// Role de client en cours de l'utilisateur Fixé pour les cartes de client.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Role de fournisseur en cours de l'utilisateur Fixé pour les cartes de fournisseur.
        /// </summary>
        public Fournisseur Fournisseur { get; set; }

        public Site Site { get; set; }

        /// <summary>
        /// Id du dernier site de fournisseur où l'utilisateur était lors de sa déconnection.
        /// 0 si l'utilisateur n'était pas sur un site de fournisseur lors de sa déconnection.
        /// </summary>
        public uint IdDernierSite { get
            {
                return Fournisseur != null ? Fournisseur.Id
                    : Client != null ? Client.SiteId
                    : 0;
            }
        }
        public int SessionId { get; set; }

        public IActionResult Erreur { get; set; }

        private readonly ApplicationContext _context;
        public CarteUtilisateur(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Complète l'Utilisateur avec ses Archives, avec ses Clients incluant leur Site incluant son Fournisseur
        /// et avec ses Fournisseurs incluant leur Site.
        /// Ne copie pas l'identifiant de session.
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns></returns>
        public async Task FixeUtilisateur(Utilisateur utilisateur)
        {
            Utilisateur = utilisateur;
            utilisateur.Archives = await _context.ArchiveUtilisateur
                .Where(a => a.Id == utilisateur.Id)
                .OrderBy(a => a.Date)
                .ToListAsync();
            utilisateur.Fournisseurs = await _context.Fournisseur
                .Where(f => f.UtilisateurId == utilisateur.Id)
                .Include(f => f.Site)
                .Include(f => f.Archives)
                .ToListAsync();
            utilisateur.Clients = await _context.Client
                .Where(c => c.UtilisateurId == utilisateur.Id)
                .Include(c => c.Site).ThenInclude(s => s.Fournisseur)
                .Include(c => c.Archives)
                .ToListAsync();
        }

        public async Task ArchiveDernierSite(uint idSite)
        {
            ArchiveUtilisateur archive = Utilisateur.Archives
                .Where(a => a.IdDernierSite != null)
                .LastOrDefault();
            if (archive == null || archive.IdDernierSite.Value != idSite)
            {
                ArchiveUtilisateur nouveau = new ArchiveUtilisateur
                {
                    Id = Utilisateur.Id,
                    Date = DateTime.Now,
                    IdDernierSite = idSite
                };
                _context.ArchiveUtilisateur.Add(nouveau);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Fournisseur> FournisseurDeClient(uint idClient)
        {
            Client client = await _context.Client.Where(c => c.Id == idClient).FirstOrDefaultAsync();
            if (client == null)
            {
                return null;
            }
            Fournisseur fournisseur = Utilisateur.Fournisseurs.Where(f => f.Id == client.SiteId).FirstOrDefault();
            return fournisseur;
        }

        public async Task<Identifiant> Identifiant()
        {
            Identifiant identifiant = new Identifiant(Utilisateur);

            if (EstAdministrateur)
            {
                return identifiant;
            }

            // listes des Id des sites de l'utilisateur dans l'ordre inverse de leurs dernières visites
            IEnumerable<uint> idSites = Utilisateur.Archives
                .Where(a => a.IdDernierSite != null)
                .GroupBy(a => a.IdDernierSite.Value)
                .Select(g => new { idSite = g.Key, date = g.OrderBy(a => a.Date).Last().Date })
                .OrderByDescending(x => x.date)
                .Select(x => x.idSite);

            foreach (uint idSite in idSites)
            {
                Fournisseur fournisseur = Utilisateur.Fournisseurs.Where(f => f.Id == idSite).FirstOrDefault();
                if (fournisseur != null)
                {
                    identifiant.Sites.Add(new SiteDIdentifiant(fournisseur));
                }
                else
                {
                    Client client = Utilisateur.Clients.Where(c => c.SiteId == idSite).FirstOrDefault();
                    identifiant.Sites.Add(new SiteDIdentifiant(client));
                }
            }

            DateTime? dateDernièreDéconnection = null;
            int sessionId = Utilisateur.SessionId; // > 0 car l'utilisateur est connecté
            sessionId--; // id de la session précédente
            if (sessionId > 0)
            {
                // si l'utilisateur s'est déconnecté de la session précédente, une archive
                // avec un SessionId opposé à celui de la session précédente a été enregistrée
                ArchiveUtilisateur dernièreDéconnection = Utilisateur.Archives
                        .Where(archive => archive.SessionId == -sessionId)
                        .FirstOrDefault();
                if (dernièreDéconnection != null)
                {
                    dateDernièreDéconnection = dernièreDéconnection.Date;
                    // on inscrit l'Id du dernier Site ou 0 si l'utilisateur était sur l'AppSite
                    identifiant.IdDernierSite = dernièreDéconnection.IdDernierSite.Value;
                }
            }

            IQueryable<DocCLF> queryNouveauxDocs = null;
            Func<DocCLF, CLFDoc> nouveauCLFDoc = null;
            foreach (SiteDIdentifiant site in identifiant.Sites)
            {
                if (site.Client == null)
                {
                    // c'est un site où l'utilisateur est Fournisseur
                    List<Produit> produits = await _context.Produit
                        .Where(produit => produit.SiteId == site.Id && produit.Disponible)
                        .ToListAsync();
                    int nbCatégories = produits.GroupBy(produit => produit.CategorieId).Count();
                    List<Client> clients = await _context.Client
                        .Where(client => client.SiteId == site.Id)
                        .ToListAsync();
                    site.Bilan = new BilanSite
                    {
                        Catalogue = new BilanCatalogue
                        {
                            Catégories = nbCatégories,
                            Produits = produits.Count()
                        },
                        Clients = new BilanClients
                        {
                            Actifs = clients.Where(client => client.Etat == EtatRole.Actif).Count(),
                            Nouveaux = clients.Where(client => client.Etat == EtatRole.Nouveau).Count()
                        }
                    };
                    queryNouveauxDocs = _context.Docs
                        .Where(docCLF => docCLF.SiteId == site.Id && docCLF.Type == TypeCLF.Commande);
                    nouveauCLFDoc = (DocCLF docCLF) => CLFDoc.DeKey(docCLF);
                }
                else
                {
                    queryNouveauxDocs = _context.Docs
                        .Where(docCLF => docCLF.SiteId == site.Id && (docCLF.Type == TypeCLF.Livraison || docCLF.Type == TypeCLF.Facture));
                    nouveauCLFDoc = (DocCLF docCLF) => CLFDoc.DeNoType(docCLF);
                }
                if (sessionId == 0)
                {
                    // c'est la première connection
                    site.NouveauxDocs = new List<CLFDoc>();
                }
                else
                {
                    // on ne joint les nouveaux docs que s'il y a eu déconnection
                    if (dateDernièreDéconnection != null)
                    {
                        List<DocCLF> nouveauxDocCLFs = await queryNouveauxDocs
                            .Where(docCLF => docCLF.Date >= dateDernièreDéconnection)
                            .ToListAsync();
                        site.NouveauxDocs = nouveauxDocCLFs
                            .Select(docCLF => nouveauCLFDoc(docCLF))
                            .ToList();
                    }
                }
            }

            DemandeSite demandeSite = await _context.DemandeSite.Where(ns => ns.Email == identifiant.Email).FirstOrDefaultAsync();
            if (demandeSite != null)
            {
                identifiant.NouveauSite = demandeSite;
            }

            return identifiant;
        }

        public bool EstUtilisateurActif
        {
            get
            {
                return Utilisateur.Etat == EtatUtilisateur.Actif || Utilisateur.Etat == EtatUtilisateur.Nouveau;
            }
        }

        public bool EstAdministrateur
        {
            get
            {
                return EstUtilisateurActif && Utilisateur.Fournisseurs.Count == 0 && Utilisateur.Clients.Count == 0;
            }
        }

    }
}
