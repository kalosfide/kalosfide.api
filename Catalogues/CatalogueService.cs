using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Produits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Catalogues
{

    public class CatalogueService : ICatalogueService
    {
        private readonly ApplicationContext _context;
        private readonly IProduitService _produitService;
        private readonly ICatégorieService _catégorieService;

        public CatalogueService(ApplicationContext context,
            IProduitService produitService,
            ICatégorieService catégorieService)
        {
            _context = context;
            _produitService = produitService;
            _catégorieService = catégorieService;
        }

        /// <summary>
        /// retourne le catalogue complet du site actuellement en vigueur
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Catalogue> Complet(Site site)
        {
            Catalogue catalogue = new Catalogue
            {
                Produits = await _produitService.ProduitsDeCatalogue(site),
                Catégories = await _catégorieService.CatégoriesDeCatalogue(site)
            };
            return catalogue;
        }

        /// <summary>
        /// retourne le catalogue des disponibilités du site actuellement en vigueur
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Catalogue> Disponibles(Site site)
        {
            Catalogue catalogue = new Catalogue
            {
                Produits = await _produitService.ProduitsDeCatalogueDisponibles(site),
                Catégories = await _catégorieService.CatégoriesDeCatalogueDesDisponibles(site)
            };
            return catalogue;
        }

        /// <summary>
        /// Termine une période de modification des données.
        /// Fixe à la date de fin la date de toutes les données modifiées depuis la date de début.
        /// Fixe à la date de fin la date de toutes les archives créées depuis la date de début.
        /// Si plusieurs de ces archives concernent la même donnée, les remplace par une seule archive résumant les modifications.
        /// Pas de SaveChanges.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="maintenant"></param>
        /// <returns>true si des modifications ont eu lieu, false sinon.</returns>
        public async Task<bool> ArchiveModifications(Site site, DateTime maintenant)
        {
            bool modifié = await _produitService.TermineModification(site, maintenant);
            modifié = modifié || await _catégorieService.TermineModification(site, maintenant);
            return modifié;
        }

    }
}
