using KalosfideAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{

    public abstract class BaseService : IBaseService
    {
        protected ApplicationContext _context;

        protected BaseService(ApplicationContext context)
        {
            _context = context;
        }

        // ERREURS


        // LECTURES

        // ECRITURE

        public async Task<RetourDeService> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new RetourDeService(TypeRetourDeService.Ok);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new RetourDeService(TypeRetourDeService.ConcurrencyError);
            }
            catch (DbUpdateException ex)
            {
                RetourDeService retour = new RetourDeService(TypeRetourDeService.UpdateError)
                {
                    Message = ex.InnerException.Message
                };
                return retour;
            }
        }

        public async Task<RetourDeService<T>> SaveChangesAsync<T>(T objet) where T: class
        {
            RetourDeService retour = await SaveChangesAsync();
            if (retour.Ok)
            {
                return new RetourDeService<T>(TypeRetourDeService.Ok)
                {
                    Objet = objet
                };
            }
            return new RetourDeService<T>(retour);
        }
    }

    public delegate IQueryable<T> InclutRelations<T>(IQueryable<T> queryT);

    public abstract class BaseService<T> : BaseService, IBaseService<T> where T : class
    {
        protected BaseService(ApplicationContext context) : base(context)
        { }

        public async Task<RetourDeService<T>> SaveChangesAsync(T donnée)
        {
            try
            {
                await _context.SaveChangesAsync();
                return new RetourDeService<T>(donnée);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new RetourDeService<T>(TypeRetourDeService.ConcurrencyError);
            }
            catch (DbUpdateException ex)
            {
                RetourDeService<T> retour = new RetourDeService<T>(TypeRetourDeService.UpdateError)
                {
                    Message = ex.InnerException.Message
                };
                return retour;
            }
        }
    }
}
