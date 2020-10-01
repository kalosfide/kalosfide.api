using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public interface IClientService : IKeyUidRnoService<Client, ClientVue>
    {

        /// <summary>
        /// retourne la liste des vues contenant les donnéees d'état des clients non exclus du site défini par la clé
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        Task<List<ClientEtatVue>> ClientsDuSite(AKeyUidRno aKeySite);

        /// <summary>
        /// retourne la liste des vues contenant les donnéees d'état des clients qui ont créé leur compte depuis la date
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        Task<List<ClientEtatVue>> NouveauxClients(AKeyUidRno aKeySite, DateTime date);

        /// <summary>
        /// retourne vrai si le client peut se connecter
        /// </summary>
        /// <param name="aKeyClient"></param>
        /// <returns></returns>
        Task<bool> AvecCompte(AKeyUidRno aKeyClient);

        /// <summary>
        /// retourne le nombre de clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        Task<int> NbClients(AKeyUidRno aKeySite);

        Task ValideAjoute(AKeyUidRno akeySite, IClient client, ModelStateDictionary modelState);
        Task<RetourDeService<Client>> Ajoute(Utilisateur utilisateur, AKeyUidRno keySite, IClient vue);

        Task ValideAjoute(ClientVueAjoute vue, ModelStateDictionary modelState);
        Task<RetourDeService<Client>> Ajoute(Utilisateur utilisateur, ClientVueAjoute vue);

        Task ValideEdite(KeyUidRno keySite, Client donnée, ModelStateDictionary modelState);
        Task<KeyParam> KeyParamDuSiteDuClient(KeyParam param);
        Task<Role> Role(KeyParam param);
        Task<RetourDeService<Role>> ChangeEtat(Role role, string état);

        Task<ClientVue> LitVue(KeyUidRno keyClient);
    }
}
