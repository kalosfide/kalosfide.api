using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Produits
{
    public interface IProduitService: IAvecIdEtSiteIdService<Produit, ProduitAAjouter, ProduitAEditer>
    {

        Task<List<ProduitDeCatalogue>> ProduitsDeCatalogue(uint idSite);
        Task<List<ProduitDeCatalogue>> ProduitsDeCatalogueDisponibles(uint idSite);

        Task<bool> NomPris(uint idSite, string nom);
        Task<bool> NomPrisParAutre(uint idSite, uint idProduit, string nom);

        /// <summary>
        /// supprime tous les détails demandant un produit si la commande n'a pas de numéro de livraison
        /// appelé quand un produit cesse d'être Disponible
        /// </summary>
        /// <param name="produit"></param>
        /// <returns></returns>
        Task SupprimeLignesCommandesPasEnvoyées(Produit produit);
    }
}
