using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catégories
{
    public interface ICatégorieService: IAvecIdEtSiteIdService<Catégorie, CatégorieAAjouter, CatégorieAEditer>
    {
        Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogue(uint idSite);
        Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogueDesDisponibles(uint idSite);
        Task<bool> NomPris(string nom);
        Task<bool> NomPrisParAutre(uint id, string nom);
    }
}
