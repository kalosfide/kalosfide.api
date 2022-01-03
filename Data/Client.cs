using KalosfideAPI.Data.Keys;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

    public class Client : AvecIdUint, IClientData, IAvecIdUintEtEtat
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

            entité.ToTable("Client");
        }

        // utile

        public static int JoursInactifAvantExclu()
        {
            return 60;
        }

        public static void CopieData(IClientData de, IClientData vers)
        {
            Role.CopieData(de, vers);
        }
        public static void CopieData(Client de, IClientDataAnnullable vers)
        {
            Role.CopieData(de, vers);
        }
        public static void CopieData(IClientDataAnnullable de, IClientDataAnnullable vers)
        {
            Role.CopieData(de, vers);
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
