﻿// <auto-generated />
using System;
using KalosfideAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KalosfideAPI.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("KalosfideAPI.Data.Administrateur", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.HasKey("Uid", "Rno");

                    b.ToTable("Administrateurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("ApplicationUser");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveCatégorie", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "No", "Date");

                    b.HasIndex("Uid", "Rno", "No");

                    b.ToTable("ArchiveCatégories");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveClient", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Adresse")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveClients");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveFournisseur", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Adresse")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveFournisseurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveProduit", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<DateTime>("Date");

                    b.Property<long?>("CategorieNo");

                    b.Property<string>("Etat")
                        .HasMaxLength(1);

                    b.Property<string>("Nom")
                        .HasMaxLength(200);

                    b.Property<decimal?>("Prix")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("TypeCommande")
                        .HasMaxLength(1);

                    b.Property<string>("TypeMesure")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "No", "Date");

                    b.HasIndex("Uid", "Rno", "No");

                    b.ToTable("ArchiveProduits");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveRole", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Etat")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveRoles");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveSite", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Etat")
                        .HasMaxLength(1);

                    b.Property<string>("FormatNomFichierCommande");

                    b.Property<string>("FormatNomFichierFacture");

                    b.Property<string>("FormatNomFichierLivraison");

                    b.Property<string>("NomSite")
                        .HasMaxLength(200);

                    b.Property<string>("Titre")
                        .HasMaxLength(200);

                    b.Property<string>("Ville");

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveSites");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveUtilisateur", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<DateTime>("Date");

                    b.Property<string>("Etat")
                        .HasMaxLength(1);

                    b.Property<int?>("NoDernierRole");

                    b.HasKey("Uid", "Date");

                    b.HasIndex("Uid");

                    b.ToTable("ArchiveUtilisateurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Catégorie", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "No");

                    b.HasIndex("Uid", "Rno", "Nom")
                        .IsUnique();

                    b.ToTable("Catégories");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Client", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<string>("Adresse")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("Nom");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("KalosfideAPI.Data.DocCLF", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<string>("Type");

                    b.Property<DateTime?>("Date");

                    b.Property<bool?>("Incomplet");

                    b.Property<int?>("NbLignes");

                    b.Property<long?>("NoGroupe");

                    b.Property<int>("SiteRno");

                    b.Property<string>("SiteUid")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<decimal?>("Total")
                        .HasColumnType("decimal(7,2)");

                    b.HasKey("Uid", "Rno", "No", "Type");

                    b.ToTable("Docs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Fournisseur", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<string>("Adresse")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("Nom");

                    b.ToTable("Fournisseurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.LigneCLF", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<string>("Uid2")
                        .HasMaxLength(20);

                    b.Property<int>("Rno2");

                    b.Property<long>("No2");

                    b.Property<string>("Type");

                    b.Property<decimal?>("AFixer")
                        .HasColumnType("decimal(8,3)");

                    b.Property<DateTime>("Date");

                    b.Property<decimal?>("Quantité")
                        .HasColumnType("decimal(8,3)");

                    b.Property<string>("TypeCommande")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "No", "Uid2", "Rno2", "No2", "Type");

                    b.HasIndex("Uid", "Rno", "No", "Type");

                    b.HasIndex("Uid2", "Rno2", "No2", "Date");

                    b.ToTable("Lignes");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Produit", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<long>("No");

                    b.Property<long>("CategorieNo");

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("D");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<decimal>("Prix")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(7,2)")
                        .HasDefaultValue(0m);

                    b.Property<string>("TypeCommande")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("1");

                    b.Property<string>("TypeMesure")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("U");

                    b.HasKey("Uid", "Rno", "No");

                    b.HasIndex("Uid", "Rno", "CategorieNo");

                    b.HasIndex("Uid", "Rno", "Nom")
                        .IsUnique();

                    b.ToTable("Produits");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Role", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("N");

                    b.Property<int>("SiteRno");

                    b.Property<string>("SiteUid")
                        .HasMaxLength(20);

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("SiteUid", "SiteRno");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Site", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(20);

                    b.Property<int>("Rno");

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("O");

                    b.Property<string>("FormatNomFichierCommande");

                    b.Property<string>("FormatNomFichierFacture");

                    b.Property<string>("FormatNomFichierLivraison");

                    b.Property<string>("NomSite")
                        .HasMaxLength(200);

                    b.Property<string>("Titre")
                        .HasMaxLength(200);

                    b.Property<string>("Ville");

                    b.HasKey("Uid", "Rno");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Utilisateur", b =>
                {
                    b.Property<string>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20);

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1)
                        .HasDefaultValue("N");

                    b.Property<string>("UserId");

                    b.HasKey("Uid");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasFilter("[UserId] IS NOT NULL");

                    b.ToTable("Utilisateurs");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Administrateur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithOne()
                        .HasForeignKey("KalosfideAPI.Data.Administrateur", "Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveCatégorie", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Catégorie", "Catégorie")
                        .WithMany("ArchiveCatégories")
                        .HasForeignKey("Uid", "Rno", "No")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveProduit", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Produit", "Produit")
                        .WithMany("ArchiveProduits")
                        .HasForeignKey("Uid", "Rno", "No")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveUtilisateur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Utilisateur", "Utilisateur")
                        .WithMany("Etats")
                        .HasForeignKey("Uid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.Catégorie", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Fournisseur", "Producteur")
                        .WithMany("Catégories")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.Client", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithOne()
                        .HasForeignKey("KalosfideAPI.Data.Client", "Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.DocCLF", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Client", "Client")
                        .WithMany("Docs")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.Fournisseur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithOne()
                        .HasForeignKey("KalosfideAPI.Data.Fournisseur", "Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.LigneCLF", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Produit", "Produit")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid2", "Rno2", "No2");

                    b.HasOne("KalosfideAPI.Data.DocCLF", "Doc")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid", "Rno", "No", "Type")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KalosfideAPI.Data.ArchiveProduit", "ArchiveProduit")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid2", "Rno2", "No2", "Date");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Produit", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Site", "Site")
                        .WithMany("Produits")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KalosfideAPI.Data.Catégorie", "Catégorie")
                        .WithMany("Produits")
                        .HasForeignKey("Uid", "Rno", "CategorieNo")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KalosfideAPI.Data.Role", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Utilisateur", "Utilisateur")
                        .WithMany("Roles")
                        .HasForeignKey("Uid")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KalosfideAPI.Data.Site", "Site")
                        .WithMany("Usagers")
                        .HasForeignKey("SiteUid", "SiteRno");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Utilisateur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser", "ApplicationUser")
                        .WithOne("Utilisateur")
                        .HasForeignKey("KalosfideAPI.Data.Utilisateur", "UserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KalosfideAPI.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
