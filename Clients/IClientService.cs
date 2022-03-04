using KalosfideAPI.Data;
using KalosfideAPI.Partages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
{
    public interface IClientService: IAvecIdUintService<Client, ClientAAjouter, ClientEtatVue, ClientAEditer>
    {

        /// <summary>
        /// Retourne la liste des vues contenant les donnéees d'état des clients d'un site ayant un Etat permis
        /// et n'ayant pas d'Utilisateur ou ayant un Utilisateur d'Etat permis.
        /// </summary>
        /// <param name="idSite">Id du site</param>
        /// <param name="permissionsClient">Array des EtatClient permis</param>
        /// <param name="permissionsUtilisateur">Array des EtatUtilisateur permis</param>
        /// <returns></returns>
        Task<List<ClientEtatVue>> ClientsDuSite(uint idSite, PermissionsEtatRole permissionsClient, PermissionsEtatUtilisateur permissionsUtilisateur);

        /// <summary>
        /// Retourne l'email de l'utilsateur si le client gère son compte
        /// </summary>
        /// <param name="idClient">objet ayant la clé du client</param>
        /// <returns>null si le client ne gère pas son compte</returns>
        Task<string> Email(uint idClient);

        /// <summary>
        /// Cherche un Client d'un site à partir de l'Email de son Utilisateur.
        /// </summary>
        /// <param name="idSite">Id d'un Site</param>
        /// <param name="email">adresse email</param>
        /// <returns>le Client trouvé, s'il y en a un; null, sinon.</returns>
        Task<Client> ClientDeEmail(uint idSite, string email);

        /// <summary>
        /// Cherche dans la bdd l'Invitation à partir de son IInvitationKey (Id du Fournisseur, Email de l'invité).
        /// </summary>
        /// <param name="invitationKey">IInvitationKey (Id du Fournisseur, Email de l'invité)</param>
        /// <returns>l'Invitation trouvée si elle existe; null sinon</returns>
        Task<Invitation> LitInvitation(IInvitationKey invitationKey);

        /// <summary>
        /// Cherche dans la bdd une Invitation à prendre en charge un client.
        /// </summary>
        /// <param name="idClient">Id du Client recherché</param>
        /// <returns>l'invitation trouvée avec le Client à prendre en charge, si elle existe; null, sinon.</returns>
        Task<Invitation> InvitationDeClientId(uint idClient);

        /// <summary>
        /// Envoie un message à l'Email de l'invitation avec un lien contenant l'Invitation encodée.
        /// </summary>
        /// <param name="invitation">Invitation à envoyer</param>
        /// <param name="fournisseur">Fournisseur qui envoie l'Invitation</param>
        /// <param name="client">Client existant éventuel à prendre en charge par l'invité</param>
        /// <returns></returns>
        Task EnvoieEmailInvitation(Invitation invitation, Fournisseur fournisseur, Client client);

        /// <summary>
        /// Invitation contenue dans le code du lien envoyé dans le message email d'invitation.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>l'Invitation contenue dans le code, si le code est valide; null, sinon</returns>
        Invitation DécodeInvitation(string code);

        /// <summary>
        /// Enregistre dans la bdd l'envoi d'une Invitation.
        /// </summary>
        /// <param name="invitation">Invitation envoyée</param>
        /// <param name="enregistrée">enregistrement dans la bdd d'une Invitation précédente envoyée par le même Fournisseur au même Email</param>
        /// <returns></returns>
        Task<RetourDeService> EnregistreInvitation(Invitation invitation, Invitation enregistrée);

        /// <summary>
        /// Supprime une Invitation de la bdd.
        /// </summary>
        /// <param name="invitation">Invitation à supprimer</param>
        /// <returns></returns>
        Task<RetourDeService> SupprimeInvitation(Invitation invitation);

        /// <summary>
        /// Vérifie qu'il y a dans la bdd une Invitation identique à une invitation transmise par l'UI.
        /// </summary>
        /// <param name="invitation">Invitation transmise par l'UI</param>
        /// <returns>l'invitation trouvée avec son Fournisseur incluant son Site et éventuellement le Client à prendre en charge
        /// si elle existe; null sinon</returns>
        Task<Invitation> InvitationEnregistrée(Invitation invitation);

        /// <summary>
        /// Liste des Invitations d'un Site sans leur Id qui est celle du Fournisseur
        /// </summary>
        /// <param name="idSite"></param>
        /// <returns></returns>
        Task<List<InvitationVue>> InvitationsSansId(uint idSite);

        /// <summary>
        /// Crée un nouveau Client et si il y a un ancien Client attribue ses archives et ses documents au client créé.
        /// </summary>
        /// <param name="idSite">Id du Site</param>
        /// <param name="idUtilisateur">Id de l'Utilisateur</param>
        /// <param name="vue">Données du Client à créer</param>
        /// <param name="clientInvité">Client créé par le fournisseur que le nouveau Client va prendre en charge</param>
        /// <param name="modelState">ModelStateDictionary où inscrire les erreurs de validation</param>
        /// <returns></returns>
        Task<RetourDeService> CréeClient(uint idSite, string idUtilisateur, IClientData vue, Client clientInvité, ModelStateDictionary modelState);

        /// <summary>
        /// Lit dans le bdd un Client avec Site et Utilisateur.
        /// </summary>
        /// <param name="idClient">Id du Client</param>
        /// <returns></returns>
        Task<Client> LitClient(uint idClient);

        /// <summary>
        /// Change l'Etat du Client en Actif et sauvegarde
        /// </summary>
        /// <param name="clientNonActif">Client d'état différent de Actif</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état</returns>
        Task<RetourDeService<ClientEtatVue>> Active(Client clientNonActif);

        /// <summary>
        /// Supprime toutes les modifications apportées à la bdd depuis et y compris la création du Client sur Invitation
        /// </summary>
        /// <param name="clientNouveau">Client qui a été créé en répondant à une Invitation</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant un Client identique à celui que l'Invitation invitait à gérer s'il y en avait un, null sinon</returns>
        new Task<RetourDeService<ClientEtatVue>> Supprime(Client clientNouveau);

        /// <summary>
        /// Si le Client a été créé par le fournisseur et s'il y a des documents avec des lignes, change son Etat en Fermé.
        /// Si le Client a été créé par le fournisseur et est vide, supprime le Client.
        /// Si le Client a été créé en répondant à une invitation, change son Etat en Inactif et il passera automatiquement à l'état Fermé
        /// quand le client se connectera ou quand le fournisseur chargera la liste des clients aprés 60 jours.
        /// </summary>
        /// <param name="clientActif">Client d'état Actif</param>
        /// <returns>RetourDeService d'un ClientEtatVue contenant uniquement la clé et la date de changement d'état ou null si le Client a été supprimé</returns>
        Task<RetourDeService<ClientEtatVue>> Inactive(Client clientActif);
    }
}
