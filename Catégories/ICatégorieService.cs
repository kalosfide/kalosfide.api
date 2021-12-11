using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using KalosfideAPI.Produits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catégories
{
    public interface ICatégorieService: IKeyUidRnoNoService<Catégorie, CatégorieVue>
    {
        Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogue(AKeyUidRno aKeySite);
        Task<List<CatégorieDeCatalogue>> CatégoriesDeCatalogueDesDisponibles(AKeyUidRno aKeySite);
        Task<bool> NomPris(string nom);
        Task<bool> NomPrisParAutre(AKeyUidRnoNo key, string nom);
    }
}
