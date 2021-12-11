using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data1
{
    public class Site
    {
        public int Id { get; set; }

        public int FournisseurId { get; set; }

        public virtual Fournisseur Fournisseur { get; set; }
        public virtual ICollection<Client> Clients { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Site>();


            entité.ToTable("Site");
        }
    }
}
