using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Documents
{
    public interface IDocumentService : IBaseService
    {
        Task<Documents> ListeC(AKeyUidRno keySite, AKeyUidRno keyClient);
        Task<Documents> ListeF(AKeyUidRno keySite);
        Task<AKeyUidRnoNo> Commande(AKeyUidRno keySite, KeyUidRnoNo keyDocument);
        Task<AKeyUidRnoNo> Livraison(AKeyUidRno keySite, KeyUidRnoNo keyDocument);
        Task<AKeyUidRnoNo> Facture(AKeyUidRno keySite, KeyUidRnoNo keyDocument);    }
}
