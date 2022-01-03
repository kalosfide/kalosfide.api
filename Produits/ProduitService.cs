using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Erreurs;
using KalosfideAPI.Partages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    class GèreArchive : AvecIdEtSiteIdGèreArchive<Produit, ProduitAEditer, ArchiveProduit>
    {
        public GèreArchive(DbSet<ArchiveProduit> dbSetArchive) : base(dbSetArchive)
        {
        }

        protected override ArchiveProduit CréeArchive()
        {
            return new ArchiveProduit();
        }

        protected override void CopieDonnéeDansArchive(Produit produit, ArchiveProduit archive)
        {
            Produit.CopieData(produit, archive);
            archive.Disponible = produit.Disponible;
        }

        protected override ArchiveProduit CréeArchiveDesDifférences(Produit donnée, ProduitAEditer vue)
        {
            ArchiveProduit archive = new ArchiveProduit()
            {
                Date = DateTime.Now
            };
            bool modifié = Produit.CopieDifférences(donnée, vue, archive);
            return modifié ? archive : null;
        }

        protected override IQueryable<ArchiveProduit> ArchivesAvecDonnée(uint idSite)
        {
            return _dbSetArchive
                .Include(a => a.Produit)
                .Where(a => a.Produit.SiteId == idSite);
        }

        protected override Produit DonnéeDeArchive(ArchiveProduit archive)
        {
            return archive.Produit;
        }

        protected override void CopieArchiveDansArchive(ArchiveProduit de, ArchiveProduit vers)
        {
            Produit.CopieDataSiPasNull(de, vers);
        }
    }

    public class ProduitService : AvecIdEtSiteIdService<Produit, ProduitAAjouter, ProduitAEditer>, IProduitService
    {
        public ProduitService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Produit;
            _gèreArchive = new GèreArchive(_context.ArchiveProduit);
            dValideAjoute = ValideAjoute;
            dValideEdite = ValideEdite;
            dValideSupprime = ValideSupprime;
        }
        private bool VérifieTypeUnités(Produit donnée, ModelStateDictionary modelState)
        {
            bool valide = donnée.TypeCommande == TypeCommande.Unité ? donnée.TypeMesure == TypeMesure.Aucune : donnée.TypeMesure != TypeMesure.Aucune;
            if (valide)
            {
                return true;
            }
            ErreurDeModel.AjouteAModelState(modelState, "typesMesureEtCommande");
            return false;
        }

        private bool VérifiePrix(Produit donnée, ModelStateDictionary modelState)
        {
            string erreur = PrixProduitDef.Vérifie(donnée.Prix);
            if (erreur == null)
            {
                return true;
            }
            ErreurDeModel.AjouteAModelState(modelState, "prix", erreur);
            return false;
        }

        private async Task ValideAjoute(Produit donnée, ModelStateDictionary modelState)
        {
            if (VérifieTypeUnités(donnée, modelState))
            {
                if (VérifiePrix(donnée, modelState))
                {
                    if (await NomPris(donnée.Id, donnée.Nom))
                    {
                        ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
                    }
                }
            }
        }

        private async Task ValideEdite(Produit donnée, ModelStateDictionary modelState)
        {
            if (VérifieTypeUnités(donnée, modelState))
            {
                if (VérifiePrix(donnée, modelState))
                {
                    if (await NomPrisParAutre(donnée.SiteId, donnée.Id, donnée.Nom))
                    {
                        ErreurDeModel.AjouteAModelState(modelState, "nom", "nomPris");
                    }
                }
            }
        }

        private async Task ValideSupprime(Produit donnée, ModelStateDictionary modelState)
        {
            bool avecCommandes = await _context.Lignes
                .Where(l => l.ProduitId == donnée.Id)
                .AnyAsync();
            if (avecCommandes)
            {
                ErreurDeModel.AjouteAModelState(modelState, "supprime");
            }
        }

        protected override void CopieAjoutDansDonnée(ProduitAAjouter de, Produit vers)
        {
            vers.SiteId = de.SiteId;
            Produit.CopieData(de, vers);
            vers.Disponible = de.Disponible;
        }

        protected override void CopieEditeDansDonnée(ProduitAEditer de, Produit vers)
        {
            Produit.CopieDataSiPasNull(de, vers);
        }

        protected override void CopieVuePartielleDansDonnée(ProduitAEditer de, Produit vers, Produit pourCompléter)
        {
            Produit.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
        }

        public override Produit CréeDonnée()
        {
            return new Produit
            {
                // la date sera mise à jour à la fin de la modification

                Date = DateTime.Now
            };
        }

        public async Task<bool> NomPris(uint idSite, string nom)
        {
            return await _dbSet
                .Where(produit => produit.SiteId == idSite && produit.Nom == nom)
                .AnyAsync();
        }

        public async Task<bool> NomPrisParAutre(uint idSite, uint idProduit, string nom)
        {
            return await _dbSet
                .Where(produit => produit.SiteId == idSite && produit.Nom == nom && produit.Id != idProduit)
                .AnyAsync();
        }

        public async Task<List<ProduitDeCatalogue>> ProduitsDeCatalogue(uint idSite)
        {
            List<Produit> produits = await _context.Produit
                .Where(p => p.SiteId == idSite)
                .Include(p => p.Lignes)
                .ToListAsync();
            return produits.Select(p => ProduitDeCatalogue.AvecEtat(p)).ToList();
        }

        public async Task<List<ProduitDeCatalogue>> ProduitsDeCatalogueDisponibles(uint idSite)
        {
            List<Produit> produits = await _context.Produit
                .Where(p => p.SiteId == idSite && p.Disponible)
                .Include(p => p.Lignes)
                .ToListAsync();
            return produits.Select(p => ProduitDeCatalogue.SansEtatNiDate(p)).ToList();
        }

        /// <summary>
        /// supprime toutes les lignes demandant un produit si la commande n'est pas envoyée
        /// appelé quand un produit cesse d'être Disponible
        /// </summary>
        /// <param name="produit"></param>
        /// <returns></returns>
        public async Task SupprimeLignesCommandesPasEnvoyées(Produit produit)
        {
            List<DocCLF> commandesPasEnvoyées = await _context.Docs
                .Where(d => d.Date.HasValue && d.Type == TypeCLF.Commande && d.SiteId == produit.SiteId)
                .Include(d => d.Lignes)
                .ToListAsync();
            List<LigneCLF> lignes = commandesPasEnvoyées.Aggregate(new List<LigneCLF>(),
                (List<LigneCLF> liste, DocCLF doc) =>
                {
                    liste.AddRange(doc.Lignes.Where(ligne => ligne.ProduitId == produit.Id));
                    return liste;
                });
            _context.Lignes.RemoveRange(lignes);
            await _context.SaveChangesAsync();
        }

    }
}
