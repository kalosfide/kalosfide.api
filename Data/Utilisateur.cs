using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{

    public class Utilisateur: IKeyUid
    {
        [Required]
        [MaxLength(LongueurMax.UId)]
        public string Uid { get; set; }

        // données
        public string UserId { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

        /// <summary>
        /// Vaut 0 si l'utilisateur ne s'est jamais connecté.
        /// Augmente de 1 à chaque connection.
        /// Est changé en son opposé à chaque déconnection. 
        /// </summary>
        public int SessionId { get; set; }

        // navigation
        virtual public ApplicationUser ApplicationUser { get; set; }

        virtual public ICollection<Role> Roles { get; set; }
        virtual public ICollection<ArchiveUtilisateur> Archives { get; set; }

        // utiles

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Utilisateur>();

            entité.HasKey(utilisateur => utilisateur.Uid);

            entité.Property(donnée => donnée.Etat).HasDefaultValue(TypeEtatUtilisateur.Nouveau);
            entité.Property(donnée => donnée.SessionId).HasDefaultValue(0);

            entité.ToTable("Utilisateurs");
        }
    }

}
