using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data1
{
    public class Fournisseur
    {
        public int Id { get; set; }

        public string UtilisateurId { get; set; }

        public virtual Utilisateur Utilisateur { get; set; }

        public virtual Site Site { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Fournisseur>();

            entité.HasOne(f => f.Site).WithOne(s => s.Fournisseur).HasForeignKey<Site>(f => f.Id).HasPrincipalKey<Fournisseur>(s => s.Id);
            entité.HasOne(f => f.Utilisateur).WithMany(u => u.Fournisseurs).HasForeignKey(f => f.UtilisateurId).HasPrincipalKey(u => u.Id);

            entité.ToTable("Fournisseur");
        }
    }
}
