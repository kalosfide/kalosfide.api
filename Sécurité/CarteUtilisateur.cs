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

    public class CarteUtilisateur // pour l'Api et pour l'application cliente
    {
        public Utilisateur Utilisateur { get; set; }
        public Role Role { get; set; }
        /// <summary>
        /// Rno du role correspondant au site de fournisseur où l'utilisateur était lors de sa déconnection.
        /// 0 si l'utilisateur n'était pas sur un site de fournisseur lors de sa déconnection.
        /// </summary>
        public int NoDernierRole { get; set; }
        public int SessionId { get; set; }

        public IActionResult Erreur { get; set; }

        private readonly ApplicationContext _context;
        public CarteUtilisateur(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<Identifiant> Identifiant()
        {
            Identifiant identifiant = new Identifiant(Utilisateur);

            if (EstAdministrateur)
            {
                return identifiant;
            }

            DateTime? dateDernièreDéconnection = null;
            int sessionId = Utilisateur.SessionId; // > 0 car l'utilisateur est connecté
            sessionId--; // id de la session précédente
            if (sessionId == 0)
            {
                // c'est la première connection
                identifiant.NoDernierRole = Utilisateur.Roles.First().Rno;
            }
            else
            {
                identifiant.NoDernierRole = NoDernierRole;
                // si l'utilisateur s'est déconnecté de la session précédente, une archive
                // avec un SessionId opposé à celui de la session précédente a été enregistrée
                ArchiveUtilisateur dernièreDéconnection = await _context.ArchiveUtilisateur
                        .Where(archive => archive.Uid == Utilisateur.Uid && archive.SessionId == -sessionId)
                        .FirstOrDefaultAsync();
                if (dernièreDéconnection != null)
                {
                    dateDernièreDéconnection = dernièreDéconnection.Date;
                }
            }
            IQueryable<DocCLF> queryNouveauxDocs = null;
            Func<DocCLF, CLFDoc> nouveauCLFDoc = null;
            foreach (Role role in Utilisateur.Roles)
            {
                RoleDIdentifiant roleDIdentifiant = new RoleDIdentifiant(role);
                string uidSite = role.Site.Uid;
                int rnoSite = role.Site.Rno;
                SiteDeRole siteDeRole = roleDIdentifiant.Site;
                if (Role.EstFournisseur(role))
                {
                    List<Produit> produits = await _context.Produit
                        .Where(produit => produit.Uid == uidSite && produit.Rno == rnoSite && produit.Etat == TypeEtatProduit.Disponible)
                        .ToListAsync();
                    int nbCatégories = produits.GroupBy(produit => produit.CategorieNo).Count();
                    List<Role> clients = await _context.Role
                        .Where(role => role.SiteUid == uidSite && role.SiteRno == rnoSite && (role.Uid != uidSite || role.Rno != rnoSite))
                        .ToListAsync();
                    siteDeRole.Bilan = new BilanSite
                    {
                        Catalogue = new BilanCatalogue
                        {
                            Catégories = nbCatégories,
                            Produits = produits.Count()
                        },
                        Clients = new BilanClients
                        {
                            Actifs = clients.Where(client => client.Etat == TypeEtatRole.Actif).Count(),
                            Nouveaux = clients.Where(client => client.Etat == TypeEtatRole.Nouveau).Count()
                        }
                    };
                    queryNouveauxDocs = _context.Docs
                        .Where(docCLF => docCLF.SiteUid == role.Uid && docCLF.SiteRno == role.Rno
                            && docCLF.Type == TypeClf.Commande);
                    nouveauCLFDoc = (DocCLF docCLF) => CLFDoc.DeKey(docCLF);
                }
                else
                {
                    Role fournisseur = await _context.Role
                        .Where(r => r.Uid == uidSite && r.Rno == rnoSite)
                        .FirstOrDefaultAsync();
                    siteDeRole.Fournisseur = new FournisseurDeSiteDeRole(fournisseur);
                    queryNouveauxDocs = _context.Docs
                        .Where(docCLF => docCLF.Uid == role.Uid && docCLF.Rno == role.Rno
                            && (docCLF.Type == TypeClf.Livraison || docCLF.Type == TypeClf.Facture));
                    nouveauCLFDoc = (DocCLF docCLF) => CLFDoc.DeNoType(docCLF);
                }
                if (sessionId == 0)
                {
                    // c'est la première connection
                    siteDeRole.NouveauxDocs = new List<CLFDoc>();
                }
                else
                {
                    // on ne joint les nouveaux docs que s'il y a eu déconnection
                    if (dateDernièreDéconnection != null)
                    {
                        List<DocCLF> nouveauxDocCLFs = await queryNouveauxDocs
                            .Where(docCLF => docCLF.Date >= dateDernièreDéconnection)
                            .ToListAsync();
                        siteDeRole.NouveauxDocs = nouveauxDocCLFs
                            .Select(docCLF => nouveauCLFDoc(docCLF))
                            .ToList();
                    }
                }
                identifiant.Roles.Add(roleDIdentifiant);
            }

            NouveauSite nouveauSite = await _context.NouveauxSites.Where(ns => ns.Email == identifiant.Email).FirstOrDefaultAsync();
            if (nouveauSite != null)
            {
                identifiant.NouveauSite = nouveauSite;
            }

            return identifiant;
        }

        /// <summary>
        /// Copie les données de l'utilisateur, crée les listes des roles et des sites dont l'utilisateur est usager.
        /// Copie le numéro du dernier role utilisé archivé.
        /// Ne copie pas l'identifiant de session.
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns></returns>
        public async Task FixeUtilisateur(Utilisateur utilisateur)
        {
            Utilisateur = utilisateur;
            ArchiveUtilisateur archive = await _context.ArchiveUtilisateur
                .Where(au => au.Uid == Utilisateur.Uid && au.NoDernierRole.HasValue)
                .OrderBy(au => au.Date)
                .LastOrDefaultAsync();
            if (archive != null)
            {
                NoDernierRole = archive.NoDernierRole.Value;
            }
        }

        /// <summary>
        /// Met éventuellement à jour le NoDernierRole archivé de l'utilisateur.
        /// </summary>
        /// <returns></returns>
        public async Task FixeNoDernierRole()
        {
            if (Role.Rno != NoDernierRole)
            {
                NoDernierRole = Role.Rno;
                ArchiveUtilisateur archiveUtilisateur = new ArchiveUtilisateur
                {
                    Uid = Utilisateur.Uid,
                    Date = DateTime.Now,
                    NoDernierRole = Role.Rno
                };
                _context.ArchiveUtilisateur.Add(archiveUtilisateur);
                await _context.SaveChangesAsync();
            }

        }

        public void AjouteRole(Role fournisseur)
        {
            Utilisateur.Roles.Add(fournisseur);
            NoDernierRole = fournisseur.Rno;
        }

        public bool EstUtilisateurActif
        {
            get
            {
                return Utilisateur.Etat == TypeEtatUtilisateur.Actif || Utilisateur.Etat == TypeEtatUtilisateur.Nouveau;
            }
        }

        public bool EstAdministrateur
        {
            get
            {
                return EstUtilisateurActif && Utilisateur.Roles.Count == 0;
            }
        }

        private async Task FixeNoDernierRole(int Rno)
        {
            ArchiveUtilisateur archive = await _context.ArchiveUtilisateur
                .Where(a => a.Uid == Utilisateur.Uid && a.NoDernierRole.HasValue)
                .OrderBy(a => a.Date)
                .LastAsync();
            if (archive.NoDernierRole.Value != NoDernierRole)
            {
                ArchiveUtilisateur archiveUtilisateur = new ArchiveUtilisateur
                {
                    Uid = Utilisateur.Uid,
                    Date = DateTime.Now,
                    NoDernierRole = Rno
                };
                _context.ArchiveUtilisateur.Add(archiveUtilisateur);
                await _context.SaveChangesAsync();
                NoDernierRole = Rno;
            }

        }

        public async Task<bool> EstFournisseurActif(AKeyUidRno akeySite)
        {
            if (EstUtilisateurActif && akeySite.Uid == Utilisateur.Uid)
            {
                Role role = Utilisateur.Roles.Where(r => r.Rno == akeySite.Rno).FirstOrDefault();
                if (role == null || role.Etat != TypeEtatRole.Actif)
                {
                    return false;
                }
                await FixeNoDernierRole(role.Rno);
                return true;
            }
            return false;
        }

        public async Task<bool> EstClientActif(AKeyUidRno akeySite)
        {
            if (EstUtilisateurActif)
            {
                Role role = Utilisateur.Roles
                    .Where(r => r.SiteUid == akeySite.Uid && r.SiteRno == akeySite.Rno)
                    .FirstOrDefault();
                if (role == null || role.Etat != TypeEtatRole.Actif)
                {
                    return false;
                }
                if (role.Uid == akeySite.Uid && role.Rno == akeySite.Rno)
                {
                    // c'est le fournisseur
                    return false;
                }
                await FixeNoDernierRole(role.Rno);
                return true;
            }
            return false;
        }

    }
}
