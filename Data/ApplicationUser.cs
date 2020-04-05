using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        // navigation
        virtual public Utilisateur Utilisateur { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ApplicationUser>();

            entité
                .HasOne(user => user.Utilisateur)
                .WithOne(utilisateur => utilisateur.ApplicationUser)
                .HasForeignKey<Utilisateur>(utilisateur => utilisateur.UserId);

            entité.ToTable("ApplicationUser");
        }
    }
}
