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
        Task<DocCLF> DocCLFDeKey(IKeyDocSansType doc, TypeCLF type);

        /// <summary>
        /// Retourne la ligne définie par la clé et le type avec son document.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<LigneCLF> LigneCLFDeKey(IKeyLigneSansType keyLigne, TypeCLF type);

        /// <summary>
        /// Lit le dernier document du client défini par la clé et du type donné.
        /// </summary>
        /// <param name="idClient">id du client</param>
        /// <param name="type">TypeCLF du document</param>
        /// <returns>le DocCLF enregistré incluant ses LigneCLF et leurs produits</returns>
        Task<DocCLF> DernierDoc(uint idClient, TypeCLF type);

        Task<bool> EstSynthèseSansBons(DocCLF synthèse);

        /// <summary>
        /// Retourne la liste des documents d'un client du type demandé qui ont été envoyés (i.e. qui ont une Date) et qui ne font pas
        /// déjà partie d'une synthèse (i.e. qui n'ont pas de NoGroupe) et dont le No est dans une liste.
        /// S'il existe, le bon virtuel est considéré comme envoyé et sans synthèse.
        /// Les DocCLF retounés incluent leurs LigneCLF.
        /// </summary>
        /// <param name="paramsSynthèse">a la clé du client et contient la liste des No des documents à synthétiser</param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<List<DocCLF>> DocumentsEnvoyésSansSynthèse(ParamsSynthèse paramsSynthèse, TypeCLF type);

        /// <summary>
        /// Lit la dernière commande du client
        /// </summary>
        /// <param name="idClient">Id du client</param>
        /// <returns>un CLFDocs dont le champ Documents contient le CLFDoc de la dernière commande du client</returns>
        Task<CLFDocs> CommandeEnCours(uint idClient);
        
        /// <summary>
        /// Recherche un Client à partir de son Id.
        /// </summary>
        /// <param name="idClient">Id du client recherché</param>
        /// <returns>un Client incluant son Site avec son Fournisseur, si trouvé; null, sinon.</returns>
        Task<Client> ClientAvecSite(uint idClient);

        Task<RetourDeService<DocCLF>> AjouteBon(uint idClient, TypeCLF type, uint noDoc);
        /// <summary>
        /// Enregistre comme lignes d'un nouveau bon des copies des lignes d'un document précédent
        /// dont le produit est toujours disponible en mettant à jour s'il y a lieu la date du produit applicable.
        /// </summary>
        /// <param name="bon">nouveau bon auquel on veut ajouter des lignes</param>
        /// <param name="docACopier">document incluant ses lignes</param>
        /// <returns></returns>
        Task<RetourDeService> CopieLignes(DocCLF bon, DocCLF docACopier);

        Task<RetourDeService> EffaceBonEtSupprimeSiVirtuel(DocCLF doc);

        Task<RetourDeService> AjouteLigneCommande(Produit produit, CLFLigne ligne);

        Task<RetourDeService> EditeLigne(LigneCLF ligne, CLFLigne lignePostée);

        Task<RetourDeService<LigneCLF>> FixeLigne(LigneCLF ligne, decimal àFixer);

        Task<RetourDeService> SupprimeLigne(LigneCLF ligne);

        Task<RetourDeService<CLFDoc>> EnvoiCommande(Site site, DocCLF doc);

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="idSite"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> ClientsAvecBons(uint idSite, TypeCLF type);

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// </summary>
        /// <param name="site"></param>
        /// <param name="idClient"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> BonsDUnClient(Site site, uint idClient, TypeCLF type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne du document défini par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> CopieQuantité(IKeyDocSansType keyDoc, TypeCLF type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour la ligne définie par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        Task<RetourDeService> CopieQuantité(IKeyLigneSansType keyLigne, TypeCLF type);

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne des documents de la liste.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> CopieQuantité(List<DocCLF> docs, TypeCLF type);

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        Task<RetourDeService> Annule(IKeyLigneSansType keyLigne, TypeCLF type);

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type dont le AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> Annule(IKeyDocSansType keyDoc, TypeCLF type);

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents de la liste.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        Task<RetourDeService> Annule(List<DocCLF> docs, TypeCLF type);

        /// <summary>
        /// Crée un document de synthèse à partir des documents de la liste. Fixe le NoGroupe des documents de la liste.
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="idClient"></param>
        /// <param name="docCLFs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<RetourDeService<DocCLF>> Synthèse(Site site, uint idClient, List<DocCLF> docCLFs, TypeCLF type);

        /// <summary>
        /// Retourne la liste par client des bilans (nombre et total des montants) des documents par type.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<List<CLFClientBilanDocs>> ClientsAvecBilanDocuments(Site site);

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
        /// <param name="keyDocument"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CLFDocs> Document(KeyDocSansType keyDocument, TypeCLF type);
        Task<CLFPdfAEnvoyer> CLFPdfAEnvoyer(KeyDoc keyDocument, bool utilisateurEstLeClient);
        Task<RetourDeService> Téléchargement(KeyDoc keyDocument, bool utilisateurEstLeClient);

        /// <summary>
        /// Cherche un document de type livraison ou facture à partir de la key de son site, de son Type et de son No.
        /// </summary>
        /// <param name="paramsChercheDoc">key du site, no et type du document</param>
        /// <returns>un CLFDoc contenant la key et le nom du client et la date si le document recherché existe, null sinon</returns>
        Task<CLFDoc> ChercheDocument(ParamsChercheDoc paramsChercheDoc);

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés à l'utilisateur
        /// depuis sa dernière déconnection (bons de commande pour les sites dont l'utilisateur est fournisseur,
        /// bons de livraison et factures pour les sites dont l'utilisateur est client).
        /// La liste est dans l'ordre des dates.
        /// </summary>
        /// <param name="utilisateur">inclut les roles avec leurs site</param>
        /// <returns></returns>
        Task<CLFDocs> NouveauxDocs(Utilisateur utilisateur);

    }
}
