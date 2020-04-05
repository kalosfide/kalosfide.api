using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{
    public class Fournisseur: AKeyUidRno
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nom { get; set; }
        [MaxLength(500)]
        public string Adresse { get; set; }

        // navigation
        virtual public Role Role { get; set; }

        [InverseProperty("Producteur")]
        virtual public ICollection<Catégorie> Catégories { get; set; }

        // utiles

        public Site Site { get => Role.Site; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Fournisseur>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno });

            entité
                .HasOne(fournisseur => fournisseur.Role)
                .WithOne()
                .HasForeignKey<Fournisseur>(fournisseur => new { fournisseur.Uid, fournisseur.Rno })
                .HasPrincipalKey<Role>(role => new { role.Uid, role.Rno });

            entité.HasIndex(donnée => donnée.Nom);

            entité.ToTable("Fournisseurs");
        }

    }

}