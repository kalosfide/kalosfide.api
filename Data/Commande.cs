using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{

/// <summary>
/// La création d'une commande pour un client est possible si ce client n'a pas de dernière commande ou si cette commande a une date.
///
/// Un client avec compte a le droit de créer une commande pour lui même. 
/// Une commande créée par un client avec compte n'a pas de date à la création. Sa date est fixée à celle de l'envoi du bon de commande.
///
/// Un client avec compte a le droit de supprimer une commande, d'en créer et supprimer les détails et d'éditer les champs Demande
/// si cette commande est sa dernière commande et n'a pas de date.
/// 
/// Un client avec compte a le droit d'enregistrer le bon d'une commande si cette commande est sa dernière commande, a des détails tous avec demande et n'a pas de date.
/// Cela fixe la date de la commande. 
///
/// Une action du client est bloquée si une modification du catalogue est en cours ou a eu lieu depuis que le client a chargé la page.
/// Une navigation du client est redirigée si une modification du catalogue est en cours ou a eu lieu depuis que le client a chargé la page.
/// 
/// Le fournisseur a le droit de fixer APréparer des commandes d'un client avec date sans numéro de livraison.
/// 
/// Le fournisseur a le droit de créer une seule commande pour un client avec ou sans compte.
/// Une commande créée par le fournisseur a la date DateNulle.Date. Elle est supprimée à l'envoi du bon de livraison.
///
/// Le fournisseur a le droit de supprimer une commande d'un client, d'en créer et supprimer les détails et d'éditer les champs Demande
/// si cette commande a la date DateNulle.Date (il l'a créée).
///
/// Le fournisseur a le droit d'éditer les champs ALivrer d'une commande d'un client si cette commande est marquée APréparer.
/// 
/// Le fournisseur a le droit de créer une Livraison pour un client si ce client a des commandes marquées APréparer dont tous les champs ALivrer sont saisis.
/// Cela affecte le numéro suivant celui de la dernière livraison du site à cette livraison et à toutes ces commandes.
/// 
/// Le fournisseur a le droit de fixer APréparer des livraisons d'un client avec date sans numéro de facture.
///
/// Le fournisseur a le droit d'éditer les champs AFacturer d'une livraison si cette livraison a une facture qui n'a pas de date.
/// 
/// Le fournisseur a le droit de créer une Facture pour un client si ce client a des livraisons marquées APréparer dont tous les champs AFacturer sont saisis.
/// Cela affecte le numéro suivant celui de la dernière facture à cette facture et à toutes ces livraisons.
///
/// </summary>
    public class Commande: AKeyUidRnoNo
    {
        /// <summary>
        /// Uid du Client
        /// </summary>
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Client
        /// </summary>
        [Required]
        public override int Rno { get; set; }

        /// <summary>
        /// No de la Commande, incrémenté automatiquement, unique pour le client
        /// </summary>
        [Required]
        public override long No { get; set; }

        // données

        // la date est fixée quand le bon de commande est envoyé
        public DateTime? Date { get; set; }

        // Le Site sert à indexer
        [MaxLength(LongueurMax.UId)]
        // Uid du site
        public string SiteUid { get; set; }
        // Rno du site
        public int SiteRno { get; set; }

        /// <summary>
        /// Nombre de lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        public int? Lignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// Fixé quand le document est enregistré
        /// </summary>
        [Column(TypeName = PrixProduitDef.Type)]
        public decimal? Total { get; set; }

        /// <summary>
        /// Présent et faux si le document contient des lignes dont le coût n'est pas calculable.
        /// Fixé quand le document est enregistré
        /// </summary>
        public bool? Incomplet { get; set; }

        // fixé quand la Commande est affectée à une Livraison
        public long? LivraisonNo { get; set; }

        // navigation
        virtual public Client Client { get; set; }
        virtual public ICollection<DétailCommande> Détails { get; set; }
        virtual public Livraison Livraison { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Commande>();

            entité.HasKey(donnée => new
            {
                donnée.Uid,
                donnée.Rno,
                donnée.No
            });

            entité
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Commandes)
                .HasForeignKey(c => new { c.Uid, c.Rno });

            entité
                .HasOne(c => c.Livraison)
                .WithMany(l => l.Commandes)
                .HasForeignKey(c => new { c.Uid, c.Rno, c.LivraisonNo });

            entité.ToTable("Commandes");
        }
    }
}
