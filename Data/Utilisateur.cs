using KalosfideAPI.Data.Constantes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KalosfideAPI.Data
{

    public class Utilisateur
    {
        [Required]
        [MaxLength(LongueurMax.UId)]
        public string Uid { get; set; }

        // données
        public string UserId { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

        // navigation
        virtual public ApplicationUser ApplicationUser { get; set; }

        virtual public ICollection<Role> Roles { get; set; }
        virtual public ICollection<ArchiveUtilisateur> Etats { get; set; }

        // utiles

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Utilisateur>();

            entité.HasKey(utilisateur => utilisateur.Uid);

            entité.Property(donnée => donnée.Etat).HasDefaultValue(TypeEtatUtilisateur.Nouveau);

            entité.ToTable("Utilisateurs");
        }
    }

}
