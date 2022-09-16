using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Catégories
{
    public interface ICatégorieService: IAvecIdEtSiteIdService<Catégorie, CatégorieAAjouter, CatégorieAEnvoyer, CatégorieAEditer>
    {
        Task<List<CatégorieAEnvoyer>> CatégoriesDeCatalogue(uint idSite);
        Task<List<CatégorieAEnvoyer>> CatégoriesDeCatalogueDesDisponibles(uint idSite);
        Task<Catégorie> CatégorieDeNom(string nom);
        Task<bool> NomPris(string nom);
        Task<bool> NomPrisParAutre(uint id, string nom);
    }
}
