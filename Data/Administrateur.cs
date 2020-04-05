using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class Administrateur : AKeyUidRno
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }

        // navigation
        virtual public Role Role { get; set; }

        // utiles

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Administrateur>();

            entité.HasKey(uidRno => new { uidRno.Uid, uidRno.Rno });

            entité
                .HasOne(administrateur => administrateur.Role)
                .WithOne()
                .HasForeignKey<Administrateur>(uidRno =>  new { uidRno.Uid, uidRno.Rno })
                .HasPrincipalKey<Role>(uidRno =>  new { uidRno.Uid, uidRno.Rno });

            entité.ToTable("Administrateurs");
        }

    }

}