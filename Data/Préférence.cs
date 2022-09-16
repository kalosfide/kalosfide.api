using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public enum PréférenceId
    {
        // Documents
        /**
        * Chaînes de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        * ou du fournisseur si l'utilisateur est le client
        */
        FormatNomFichierCommande,
        FormatNomFichierLivraison,
        FormatNomFichierFacture,

        AvertissementTéléchargement,

        // Catalogue
        UsageCatégories,
        DescriptionOuvertureFermeture,
        ModalOuvertureFermeture,

        // Général
        SaisieDansTableau

    }

    public interface IPréférenceData
    {
        public uint SiteId { get; set; }
        public PréférenceId Id { get; set; }
        public string Valeur { get; set; }
    }

    public class PréférenceData : IPréférenceData
    {
        /// <summary>
        /// Si vrai, la préférence fixée par le fournisseur est un paramétre du site valable aussi pour les clients.
        /// </summary>
        public bool? PourTous { get; set; }
        public uint SiteId { get; set; }
        public PréférenceId Id { get; set; }
        public string Valeur { get; set; }
    }

    public class Préférence : IPréférenceData
    {
        public string UtilisateurId { get; set; }
        public uint SiteId { get; set; }
        public PréférenceId Id { get; set; }
        public string Valeur { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Préférence>();

            entité.HasKey(p => new { p.UtilisateurId, p.SiteId, p.Id });

            entité.ToTable("Préférences");
        }

        // utiles
        public static void CopieData(IPréférenceData de, IPréférenceData vers)
        {
            vers.SiteId = de.SiteId;
            vers.Id = de.Id;
            vers.Valeur = de.Valeur;
        }
    }
}
