using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{
    public interface IRoleData
    {
        string Nom { get; set; }
        string Adresse { get; set; }
        string Ville { get; set; }
    }

    public interface IRolePréférences
    { 
        string FormatNomFichierCommande { get; set; }
        string FormatNomFichierLivraison { get; set; }
        string FormatNomFichierFacture { get; set; }
    }

    public class Role : AKeyUidRno, IRoleData, IRolePréférences
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }

        [MaxLength(LongueurMax.UId)]
        public string SiteUid { get; set; }
        public int SiteRno { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

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

        // navigation
        virtual public Utilisateur Utilisateur { get; set; }
        virtual public ICollection<ArchiveRole> Archives { get; set; }

        [InverseProperty("Client")]
        virtual public ICollection<DocCLF> Docs { get; set; }

        virtual public Site Site { get; set; }

        // utiles

        public static bool EstAdministrateur(Role role) { return role.SiteUid == null; }
        public static bool EstFournisseur(Role role) { return role.SiteUid == role.Uid && role.SiteRno == role.Rno; }
        public static bool EstClient(Role role) { return !EstAdministrateur(role) && !EstFournisseur(role); }
        public static bool EstUsager(Role role, AKeyUidRno akeySite) { return role.SiteUid == akeySite.Uid && role.SiteRno == akeySite.Rno; }

        public static void CopieDef(IRoleData de, IRoleData vers)
        {
            vers.Nom = de.Nom;
            vers.Adresse = de.Adresse;
            vers.Ville = de.Ville;
        }

        public static void CopiePréférences(IRolePréférences de, IRolePréférences vers)
        {
            vers.FormatNomFichierCommande = de.FormatNomFichierCommande;
            vers.FormatNomFichierLivraison = de.FormatNomFichierLivraison;
            vers.FormatNomFichierFacture = de.FormatNomFichierFacture;
        }

        /// <summary>
        /// Vérifie que Nom et Adresse sont présents et non vides.
        /// </summary>
        /// <param name="roleDef"></param>
        /// <param name="modelState"></param>
        public static void VérifieTrim(IRoleData roleDef, ModelStateDictionary modelState)
        {
            if (roleDef.Nom == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "nom", "Absent");
            }
            else
            {
                roleDef.Nom = roleDef.Nom.Trim();
                if (roleDef.Nom.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "nom", "Vide");
                }
            }
            if (roleDef.Adresse == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "adresse", "Absent");
            }
            else
            {
                roleDef.Adresse = roleDef.Adresse.Trim();
                if (roleDef.Adresse.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "adresse", "Vide");
                }
            }
        }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Role>();

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Rno
            });

            entité.Property(donnée => donnée.Etat).HasDefaultValue(TypeEtatRole.Actif);

            entité.HasIndex(role => new { role.Uid, role.Rno, });

            entité.HasOne(r => r.Utilisateur).WithMany(u => u.Roles).HasForeignKey(r => r.Uid).HasPrincipalKey(u => u.Uid);

            entité.HasOne(r => r.Site).WithMany(s => s.Usagers).HasForeignKey(r => new { r.SiteUid, r.SiteRno }).HasPrincipalKey(s => new { s.Uid, s.Rno });

            entité.ToTable("Roles");
        }

    }

}