using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Roles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public interface IClientService: IRoleService
    {

        /// <summary>
        /// retourne la liste des vues contenant les donnéees d'état des clients non exclus du site défini par la clé
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        Task<List<ClientEtatVue>> ClientsDuSite(AKeyUidRno aKeySite);

        /// <summary>
        /// Retourne l'email de l'utilsateur si le client gère son compte
        /// </summary>
        /// <param name="aKeyClient">objet ayant la clé du client</param>
        /// <returns>null si le client ne gère pas son compte</returns>
        Task<string> Email(AKeyUidRno aKeyClient);

        /// <summary>
        /// retourne le nombre de clients non exclus du site
        /// </summary>
        /// <param name="aKeySite"></param>
        /// <returns></returns>
        Task<int> NbClients(AKeyUidRno aKeySite);

        /// <summary>
        /// Retourne le client du site ayant le nom
        /// </summary>
        /// <param name="akeySite"></param>
        /// <param name="nom"></param>
        /// <returns></returns>
        Task<Role> ClientDeNom(AKeyUidRno akeySite, string nom);

        /// <summary>
        /// Ajoute à la bdd un nouveau Role et l'archive correspondante
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        Task<RetourDeService<Role>> Ajoute(Utilisateur utilisateur, AKeyUidRno keySite, IRoleData vue);

        /// <summary>
        /// Ajoute à la bdd un nouveau Role et l'archive correspondante
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <param name="keySite"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        Task<RetourDeService<Role>> Ajoute(Utilisateur utilisateur, ClientVueAjoute vue);

        /// <summary>
        /// Crée un nouveau Role de Client et si il y a un ancien Client attribue ses archives et ses documents au role créé
        /// </summary>
        /// <param name="site"></param>
        /// <param name="utilisateur"></param>
        /// <param name="clientInvité"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        Task<RetourDeService> CréeRoleClient(Site site, Utilisateur utilisateur, Role clientInvité, IRoleData vue);

        /// <summary>
        /// Lit dans le bdd un Role avec Site et Utilisateur et éventuellement ApplicationUser
        /// </summary>
        /// <param name="uid">Uid du role à lire</param>
        /// <param name="rno">Rno du role à lire</param>
        /// <returns></returns>
        Task<Role> LitRole(string uid, int rno);

        /// <summary>
        /// Change l'Etat du Role en Actif et sauvegarde
        /// </summary>
        /// <param name="roleNonActif">Role d'état différent de Actif</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état</returns>
        Task<RetourDeService<ClientEtatVue>> Active(Role roleNonActif);

        /// <summary>
        /// Supprime toutes les modifications apportées à la bdd depuis et y compris la création du Role sur Invitation
        /// </summary>
        /// <param name="roleNouveau">Role qui a été créé en répondant à une Invitation</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant un Role identique à celui que l'Invitation invitait à gérer s'il y en avait un, null sinon</returns>
        new Task<RetourDeService<ClientEtatVue>> Supprime(Role roleNouveau);

        /// <summary>
        /// Si le Role a été créé par le fournisseur et s'il y a des documents avec des lignes, change son Etat en Fermé.
        /// Si le Role a été créé par le fournisseur et est vide, supprime le Role.
        /// Si le Role a été créé en répondant à une invitation, change son Etat en Inactif et il passera automatiquement à l'état Fermé
        /// quand le client se connectera ou quand le fournisseur chargera la liste des clients aprés 60 jours.
        /// </summary>
        /// <param name="roleActif">Role d'état Actif</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état ou null si le Role a été supprimé</returns>
        Task<RetourDeService<ClientEtatVue>> Inactive(Role roleActif);
    }
}
