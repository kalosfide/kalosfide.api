using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{

    /// <summary>
    /// Contient tous les champs de données hors Etat d'un Fournisseur.
    /// </summary>
    public interface IFournisseurData: IRoleData
    {
        string Siret { get; set; }
    }

    /// <summary>
    /// Contient tous les champs de données rendus nullable d'un Fournisseur.
    /// </summary>
    public interface IFournisseurDataAnnullable: IRoleDataAnnulable, IAvecEtatAnnulable
    {
        string Siret { get; set; }
    }

    public class Fournisseur: AvecIdUint, IFournisseurData, IAvecIdUintEtEtat, IRolePréférences
    {

        //données
        public string Siret { get; set; }

        /// <summary>
        /// Définit les droits du Fournisseur.
        /// Nouveau jusqu'à la fin de la première modification du catalogue.
        /// </summary>
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

        public virtual Site Site { get; set; }

        public virtual ICollection<ArchiveFournisseur> Archives { get; set; }

        public virtual ICollection<Invitation> Invitations { get; set; }

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
        public const string FormatNomFichierLivraisonParDéfaut = "livraison {no} {nom}";
        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public string FormatNomFichierFacture { get; set; }
        public const string FormatNomFichierFactureParDéfaut = "facture {no} {nom}";

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Fournisseur>();

            entité.HasKey(f => f.Id);
            entité.HasOne(f => f.Site).WithOne(s => s.Fournisseur).HasForeignKey<Site>(s => s.Id);
            entité.HasOne(f => f.Utilisateur).WithMany(u => u.Fournisseurs).HasForeignKey(f => f.UtilisateurId).HasPrincipalKey(u => u.Id);

            // Les entités Site et Fournisseur partagent une même table
            entité.ToTable("Fournisseurs");
        }
        // utile

        public static int JoursInactifAvantExclu()
        {
            return 60;
        }

        public static void CopieData(IFournisseurData de, IFournisseurData vers)
        {
            Role.CopieData(de, vers);
            vers.Siret = de.Siret;
        }
        public static void CopieData(Fournisseur de, IFournisseurDataAnnullable vers)
        {
            Role.CopieData(de, vers);
            vers.Siret = de.Siret;
            vers.Etat = de.Etat;
        }
        public static void CopieData(IFournisseurDataAnnullable de, IFournisseurDataAnnullable vers)
        {
            Role.CopieData(de, vers);
            vers.Siret = de.Siret;
            vers.Etat = de.Etat;
       }
        public static void CopieDataSiPasNull(IFournisseurDataAnnullable de, IFournisseurData vers)
        {
            Role.CopieDataSiPasNull(de, vers);
            if (de.Siret != null) { vers.Siret = de.Siret; }
        }
        public static void CopieDataSiPasNullOuComplète(IFournisseurDataAnnullable de, IFournisseurData vers, IFournisseurData pourCompléter)
        {
            Role.CopieDataSiPasNullOuComplète(de, vers, pourCompléter);
            vers.Siret = de.Siret ?? pourCompléter.Siret;
        }


        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(IFournisseurData ancien, IFournisseurDataAnnullable nouveau, IFournisseurDataAnnullable différences)
        {
            bool modifié = Role.CopieDifférences(ancien, nouveau, différences);
            if (nouveau.Siret != null && ancien.Siret != nouveau.Siret)
            {
                différences.Siret = nouveau.Siret;
                ancien.Siret = nouveau.Siret;
                modifié = true;
            }
            return modifié;
        }

        /// <summary>
        /// Fixe un IRoleEtat avec l'Etat, la date de création et la date de l'état actuel d'un fournisseur
        /// </summary>
        /// <param name="fournisseur">le Fournisseur concerné</param>
        /// <param name="roleEtat">le IRoleEtat à fixer</param>
        public static void FixeRoleEtat(Fournisseur fournisseur, IRoleEtat roleEtat)
        {
            IEnumerable<ArchiveFournisseur> archivesDansLordre = fournisseur.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            ArchiveFournisseur création = archivesDansLordre.First();
            ArchiveFournisseur actuel = archivesDansLordre.Last();
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

        public static string NomFichier(Fournisseur fournisseur, Client client, TypeCLF type, uint no)
        {
            string format = "";
            switch (type)
            {
                case TypeCLF.Commande:
                    format = fournisseur.FormatNomFichierCommande ?? FormatNomFichierCommandeParDéfaut;
                    break;
                case TypeCLF.Livraison:
                    format = fournisseur.FormatNomFichierLivraison ?? FormatNomFichierLivraisonParDéfaut;
                    break;
                case TypeCLF.Facture:
                    format = fournisseur.FormatNomFichierFacture ?? FormatNomFichierFactureParDéfaut;
                    break;
                default:
                    break;
            }
            return format.Replace("{nom}", client.Nom).Replace("{no}", no.ToString());
        }

    }
}
