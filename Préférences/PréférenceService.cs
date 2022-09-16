using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Préférences
{
    public class PréférenceService : BaseService, IPréférenceService
    {

        public PréférenceService(ApplicationContext context): base(context)
        {
        }

        public async Task<Préférence> Lit(string idUtilisateur, uint idSite, PréférenceId id)
        {
            Préférence préférence = await _context.Préférences
                .Where(p => p.UtilisateurId == idUtilisateur && p.SiteId == idSite && p.Id == id)
                .FirstOrDefaultAsync();
            return préférence;
        }

        public async Task<RetourDeService> Ajoute(Préférence préférence)
        {
            _context.Préférences.Add(préférence);
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService> FixeValeur(Préférence préférence, string valeur)
        {
            if (valeur == null)
            {
                _context.Préférences.Remove(préférence);
            }
            else
            {
                préférence.Valeur = valeur;
                _context.Préférences.Update(préférence);
            }
            return await SaveChangesAsync();
        }

        public async Task<bool> AvecCatégories(uint idSite)
        {
            Préférence préférence = await _context.Préférences
                .Where(p => p.SiteId == idSite && p.Id == PréférenceId.UsageCatégories)
                .FirstOrDefaultAsync();
            return préférence.Valeur != "0";
        }

        public string NomSansCatégorie()
        {
            return "SansCatégorie";
        }
    }
}
