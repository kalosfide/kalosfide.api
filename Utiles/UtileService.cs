using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utiles
{
    public class UtileService : IUtileService
    {
        private readonly ApplicationContext _context;

        public UtileService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// retourne le nombre de produits disponibles du site
        /// </summary>
        /// <param name="idSite">Site ou SiteVue ou keyUidRno</param>
        /// <returns></returns>
        public async Task<int> NbDisponibles(uint idSite)
        {
            return await _context.Produit
                .Where(p => p.SiteId == idSite && p.Disponible)
                .CountAsync();
        }

        /// <summary>
        /// Cherche un Produit à partir de son Id.
        /// </summary>
        /// <param name="idProduit">Id du Produit</param>
        /// <returns>le Produit qui inclut ses Archives, si trouvé; null, sinon</returns>
        public async Task<Produit> Produit(uint idProduit)
        {
            return await _context.Produit
                .Where(p => p.Id == idProduit)
                .Include(p => p.Archives)
                .FirstOrDefaultAsync();
        }

    }
}
