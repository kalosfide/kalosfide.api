﻿using KalosfideAPI.Data.Keys;
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

    public class Fournisseur: AvecIdUint, IFournisseurData, IAvecIdUintEtEtat
    {

        //données
        public string Siret { get; set; }
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

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Fournisseur>();

            entité.HasKey(f => f.Id);
            entité.HasOne(f => f.Site).WithOne().HasForeignKey<Site>(s => s.Id);
            entité.HasOne(f => f.Utilisateur).WithMany(u => u.Fournisseurs).HasForeignKey(f => f.UtilisateurId).HasPrincipalKey(u => u.Id);

            // Les entités Site et Fournisseur partagent une même table
            entité.ToTable("Fournisseur");
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
        }
        public static void CopieData(IFournisseurDataAnnullable de, IFournisseurDataAnnullable vers)
        {
            Role.CopieData(de, vers);
            vers.Siret = de.Siret;
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

    }
}
