using KalosfideAPI.Erreurs;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    public interface IBaseService
    {
        // LECTURES

        // ECRITURE

        Task<RetourDeService> SaveChangesAsync();
    }
    public interface IBaseService<T> : IBaseService where T : class
    {
        Task<RetourDeService<T>> SaveChangesAsync(T donnée);
    }
}
