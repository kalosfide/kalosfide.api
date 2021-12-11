using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data1
{
    public class Client
    {
        public int Id { get; set; }

        public string UtilisateurId { get; set; }

        public int SiteId { get; set; }

        public virtual Utilisateur Utilisateur { get; set; }

        public virtual Site Site { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Client>();

            entité.HasOne(c => c.Site).WithMany(s => s.Clients).HasForeignKey(c => c.SiteId).HasPrincipalKey(s => s.Id);
            entité.HasOne(c => c.Utilisateur).WithMany(u => u.Clients).HasForeignKey(c => c.UtilisateurId).HasPrincipalKey(u => u.Id);

            entité.ToTable("Client");
        }
    }
}
