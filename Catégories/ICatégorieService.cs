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
        CatégorieData CréeCatégorieData(ArchiveCatégorie archive);
        CatégorieData CréeCatégorieDataAvecDate(ArchiveCatégorie archive);
        Task<List<CatégorieData>> CatégorieDatas(AKeyUidRno aKeySite, List<ProduitData> produits);
        Task<bool> NomPris(string nom);
        Task<bool> NomPrisParAutre(AKeyUidRnoNo key, string nom);
    }
}
