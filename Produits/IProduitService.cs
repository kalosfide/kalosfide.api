using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public interface IProduitService: IKeyUidRnoNoService<Produit, ProduitVue>
    {

        Task<List<ProduitDeCatalogue>> ProduitsDeCatalogue(AKeyUidRno aKeySite);
        Task<List<ProduitDeCatalogue>> ProduitsDeCatalogueDisponibles(AKeyUidRno aKeySite);

        Task<bool> NomPris(string siteUid, int siteRno, string nom);
        Task<bool> NomPrisParAutre(string siteUid, int siteRno, long produitNo, string nom);

        /// <summary>
        /// supprime tous les détails demandant un produit si la commande n'a pas de numéro de livraison
        /// appelé quand un produit cesse d'être Disponible
        /// </summary>
        /// <param name="akeyProduit"></param>
        /// <returns></returns>
        Task SupprimeDétailsCommandesSansLivraison(AKeyUidRnoNo akeyProduit);
    }
}
