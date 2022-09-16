using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Préférences
{
    public interface IPréférenceService: IBaseService
    {
        Task<Préférence> Lit(string idUtilisateur, uint idSite, PréférenceId id);
        Task<RetourDeService> Ajoute(Préférence préférence);
        Task<RetourDeService> FixeValeur(Préférence préférence, string valeur);
        Task<bool> AvecCatégories(uint idSite);
        string NomSansCatégorie();
    }
}
