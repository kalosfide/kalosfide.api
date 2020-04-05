using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public interface IProduitService: IKeyUidRnoNoService<Produit, ProduitVue>
    {
        ProduitData CréeProduitDataSansEtat(ArchiveProduit archive);
        ProduitData CréeProduitDataAvecDate(ArchiveProduit archive);

        Task<List<ProduitData>> ProduitDatasDisponibles(AKeyUidRno aKeySite);
        Task<List<ProduitData>> ProduitDatas(AKeyUidRno aKeySite);

        Task<bool> NomPris(string siteUid, int siteRno, string nom);
        Task<bool> NomPrisParAutre(string siteUid, int siteRno, long produitNo, string nom);
        Task<bool> AChangé(KeyParam param);

        /// <summary>
        /// supprime tous les détails demandant un produit si la commande n'a pas de numéro de livraison
        /// appelé quand un produit cesse d'être Disponible
        /// </summary>
        /// <param name="akeyProduit"></param>
        /// <returns></returns>
        Task SupprimeDétailsCommandesSansLivraison(AKeyUidRnoNo akeyProduit);
    }
}
