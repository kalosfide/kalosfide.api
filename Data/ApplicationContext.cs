using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ApplicationContext : IdentityDbContext<Utilisateur>
    {
        public ApplicationContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            Data.Utilisateur.CréeTable(builder);

            Data.Utilisateur.CréeTable(builder);
            Data.ArchiveUtilisateur.CréeTable(builder);

            Data.Fournisseur.CréeTable(builder);
            Data.ArchiveFournisseur.CréeTable(builder);
            Data.DemandeSite.CréeTable(builder);

            Data.Site.CréeTable(builder);
            Data.ArchiveSite.CréeTable(builder);

            Data.Client.CréeTable(builder);
            Data.ArchiveClient.CréeTable(builder);

            Data.Invitation.CréeTable(builder);


            // Tables de données
            Data.Catégorie.CréeTable(builder);
            Data.Produit.CréeTable(builder);

            Data.LigneCLF.CréeTable(builder);
            Data.DocCLF.CréeTable(builder);

            // journaux
            Data.ArchiveProduit.CréeTable(builder);
            Data.ArchiveCatégorie.CréeTable(builder);
        }

        public DbSet<Utilisateur> Utilisateur { get; set; }
        public DbSet<ArchiveUtilisateur> ArchiveUtilisateur { get; set; }

        public DbSet<Fournisseur> Fournisseur { get; set; }
        public DbSet<ArchiveFournisseur> ArchiveFournisseur { get; set; }

        public DbSet<DemandeSite> DemandeSite { get; set; }

        public DbSet<Site> Site { get; set; }
        public DbSet<ArchiveSite> ArchiveSite { get; set; }

        public DbSet<Client> Client { get; set; }
        public DbSet<ArchiveClient> ArchiveClient { get; set; }

        public DbSet<Invitation> Invitation { get; set; }

        public DbSet<Catégorie> Catégorie { get; set; }
        public DbSet<ArchiveCatégorie> ArchiveCatégorie { get; set; }
        public DbSet<Produit> Produit { get; set; }
        public DbSet<ArchiveProduit> ArchiveProduit { get; set; }

        public DbSet<LigneCLF> Lignes { get; set; }
        public DbSet<DocCLF> Docs { get; set; }

    }
}
