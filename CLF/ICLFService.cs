using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public interface ICLFService
    {

        /// <summary>
        /// Retourne le document défini par la clé et le type avec ses lignes.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <summary>
        Task<DocCLF> DocCLFDeKey(AKeyUidRnoNo doc, string type);

        /// Retourne la ligne définie par la clé et le type avec son document.
        /// </summary>
        /// <param name="ligne"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<LigneCLF> LigneCLFDeKey(AKeyUidRnoNo2 ligne, string type);

        Task<DocCLF> DernierDoc(AKeyUidRno keyClient, string type);

        /// <summary>
        /// Retourne la liste des documents de la bdd du type demandé dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<List<DocCLF>> DocumentsEnvoyésSansSynthèse(CLFDocsSynthèse clfDocs, string type);

        /// <summary>
        /// Si le site est d'état Catalogue, retourne un contexte Catalogue: état site = Catalogue, date catalogue = DateNulle.
        /// Si le site est ouvert et si l'utilisateur a passé la date de son catalogue
        /// et si la date du catalogue utilisateur est postérieure à celle du catalogue de la bdd, les données utilisateur sont à jour,
        /// retourne un contexte Ok: état site = ouvert, date catalogue = DataNulle.
        /// Si le site est ouvert et si l'utilisateur a passé la date de son catalogue
        /// et si la date du catalogue utilisateur est antérieure à celle du catalogue de la bdd
        /// retourne un contexte Périmé: état site = ouvert, date catalogue = DataNulle.
        /// Si le site est ouvert et si l'utilisateur n'a pas passé la date de son catalogue, il n'y pas de données utilisateur,
        /// retourne un CLFDocs dont le champ Documents contient les données pour client de la dernière commande du client
        /// </summary>
        /// <param name="site">site du client</param>
        /// <param name="keyClient">key du client</param>
        /// <param name="dateCatalogue">présent si le client a déjà chargé les données</param>
        /// <returns></returns>
        Task<CLFDocs> CommandeEnCours(Site site, AKeyUidRno keyClient, DateTime? dateCatalogue);

        Task<RetourDeService<CLFDoc>> AjouteBon(AKeyUidRno keyClient, Site site, string type, long noDoc, DocCLF docACopier);

        Task<RetourDeService> SupprimeCommande(DocCLF doc);

        Task<RetourDeService<CLFLigne>> AjouteLigne(CLFLigne ligne);

        Task<RetourDeService<LigneCLF>> EditeLigne(LigneCLF ligne, CLFLigne lignePostée);

        Task<RetourDeService<LigneCLF>> FixeLigne(LigneCLF ligne, decimal àFixer);

        Task<RetourDeService> SupprimeLigne(LigneCLF ligne);

        Task<RetourDeService<CLFDoc>> EnvoiCommande(DocCLF doc);

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> ClientsAvecBons(Site site, string type);

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> BonsDUnClient(Site site, AKeyUidRno keyClient, string type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne du document défini par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> CopieQuantité(AKeyUidRnoNo keyDoc, string type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour la ligne définie par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        Task<RetourDeService> CopieQuantité(AKeyUidRnoNo2 keyLigne, string type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne des documents de la liste.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> CopieQuantité(List<DocCLF> docs, string type);

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        Task<RetourDeService> Annule(AKeyUidRnoNo2 keyLigne, string type);

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type dont le AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> Annule(AKeyUidRnoNo keyDoc, string type);

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents de la liste.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> Annule(List<DocCLF> docs, string type);

        /// <summary>
        /// Crée un document de synthèse à partir des documents de la liste. Fixe le NoGroupe des documents de la liste.
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// </summary>
        /// <param name="docCLFs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<RetourDeService<DocCLF>> Synthèse(List<DocCLF> docCLFs, string type);

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du site
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Site site);

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du client
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="client"></param>
        /// <returns></returns>
        Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Client client);

        /// <summary>
        /// Retourne un CLFDocs qui contient le Client du document et un Documents contenant le document avec ses lignes
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyDocument"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> Document(Site site, KeyUidRnoNo keyDocument, string type);

    }
}
