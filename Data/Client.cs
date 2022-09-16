using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KalosfideAPI.Data
{

    /// <summary>
    /// Contient tous les champs de données hors Etat d'un Client.
    /// </summary>
    public interface IClientData: IRoleData
    {
    }

    /// <summary>
    /// Contient tous les champs de données rendus nullable d'un Client.
    /// </summary>
    public interface IClientDataAnnullable: IRoleDataAnnulable
    {
    }

    public class Client : AvecIdUint, IClientData, IAvecIdUintEtEtat, IRolePréférences
    {
        // données
        public EtatRole Etat { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nom { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        [MaxLength(500)]
        public string Adresse { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

        public string UtilisateurId { get; set; }

        public virtual Utilisateur Utilisateur { get; set; }

        public uint SiteId { get; set; }

        public virtual Site Site { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public string FormatNomFichierCommande { get; set; }
        public const string FormatNomFichierCommandeParDéfaut = "{nom} commande {no}";
        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public string FormatNomFichierLivraison { get; set; }
        public const string FormatNomFichierLivraisonParDéfaut = "{nom} livraison {no}";
        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public string FormatNomFichierFacture { get; set; }
        public const string FormatNomFichierFactureParDéfaut = "{nom} facture {no}";

        virtual public ICollection<DocCLF> Docs { get; set; }

        public virtual ICollection<ArchiveClient> Archives { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Client>();

            entité.HasKey(c => c.Id);

            entité.HasOne(c => c.Site).WithMany(s => s.Clients).HasForeignKey(c => c.SiteId).HasPrincipalKey(s => s.Id);
            entité.HasOne(c => c.Utilisateur).WithMany(u => u.Clients).HasForeignKey(c => c.UtilisateurId).HasPrincipalKey(u => u.Id);
            entité.HasMany(c => c.Docs).WithOne(d => d.Client).HasForeignKey(d => d.Id).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);

            entité.ToTable("Clients");
        }

        // utile

        public static int NbJoursInactifAvantExclu = 60;

        public static void CopieData(IClientData de, IClientData vers)
        {
            Role.CopieData(de, vers);
        }
        public static void CopieData(Client de, IClientDataAnnullable vers)
        {
            Role.CopieData(de, vers);
            vers.Etat = de.Etat;
        }
        public static void CopieData(IClientDataAnnullable de, IClientDataAnnullable vers)
        {
            Role.CopieData(de, vers);
            vers.Etat = de.Etat;
        }
        public static void CopieDataSiPasNull(IClientDataAnnullable de, IClientData vers)
        {
            Role.CopieDataSiPasNull(de, vers);
        }
        public static void CopieDataSiPasNullOuComplète(IClientDataAnnullable de, IClientData vers, IClientData pourCompléter)
        {
            Role.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
        }

        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(IClientData ancien, IClientDataAnnullable nouveau, IClientDataAnnullable différences)
        {
            return Role.CopieDifférences(ancien, nouveau, différences);
        }

        /// <summary>
        /// Fixe un IRoleEtat avec l'Etat, la date de création et la date de l'état actuel d'un fournisseur
        /// </summary>
        /// <param name="client">le Fournisseur concerné</param>
        /// <param name="roleEtat">le IRoleEtat à fixer</param>
        public static void FixeRoleEtat(Client client, IRoleEtat roleEtat)
        {
            IEnumerable<ArchiveClient> archivesDansLordre = client.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            ArchiveClient création = archivesDansLordre.First();
            ArchiveClient actuel = archivesDansLordre.Last();
            roleEtat.Etat = actuel.Etat.Value;
            roleEtat.Date0 = création.Date;
            roleEtat.DateEtat = actuel.Date;
        }

        public static string[] AvérifierSansEspacesData
        {
            get
            {
                return Role.AvérifierSansEspacesData;
            }
        }

        public static string[] AvérifierSansEspacesDataAnnulable
        {
            get
            {
                return Role.AvérifierSansEspacesDataAnnulable;
            }
        }

        public static string NomFichier(Client clientAvecSiteEtFournisseur, TypeCLF type, uint no)
        {
            string format = "";
            switch (type)
            {
                case TypeCLF.Commande:
                    format = clientAvecSiteEtFournisseur.FormatNomFichierCommande ?? FormatNomFichierCommandeParDéfaut;
                    break;
                case TypeCLF.Livraison:
                    format = clientAvecSiteEtFournisseur.FormatNomFichierLivraison ?? FormatNomFichierLivraisonParDéfaut;
                    break;
                case TypeCLF.Facture:
                    format = clientAvecSiteEtFournisseur.FormatNomFichierFacture ?? FormatNomFichierFactureParDéfaut;
                    break;
                default:
                    break;
            }
            return format.Replace("{nom}", clientAvecSiteEtFournisseur.Site.Fournisseur.Nom).Replace("{no}", no.ToString());
        }

    }
}
