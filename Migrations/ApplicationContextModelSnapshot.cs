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
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("KalosfideAPI.Data.Administrateur", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.HasKey("Uid", "Rno");

                    b.ToTable("Administrateurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(256)")
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
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "No", "Date");

                    b.HasIndex("Uid", "Rno", "No");

                    b.ToTable("ArchiveCatégories");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveClient", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Adresse")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveClients");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveProduit", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<long?>("CategorieNo")
                        .HasColumnType("bigint");

                    b.Property<string>("Etat")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("Nom")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<decimal?>("Prix")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("TypeCommande")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("TypeMesure")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "No", "Date");

                    b.HasIndex("Uid", "Rno", "No");

                    b.ToTable("ArchiveProduits");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveRole", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Etat")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveRoles");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveSite", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Adresse")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Etat")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("FormatNomFichierCommande")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FormatNomFichierFacture")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FormatNomFichierLivraison")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Titre")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Ville")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Uid", "Rno", "Date");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("ArchiveSites");
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveUtilisateur", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Etat")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.Property<int?>("NoDernierRole")
                        .HasColumnType("int");

                    b.HasKey("Uid", "Date");

                    b.HasIndex("Uid");

                    b.ToTable("ArchiveUtilisateurs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Catégorie", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno", "No");

                    b.HasIndex("Uid", "Rno", "Nom")
                        .IsUnique();

                    b.ToTable("Catégories");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Client", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<string>("Adresse")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("Nom");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("KalosfideAPI.Data.DocCLF", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("Incomplet")
                        .HasColumnType("bit");

                    b.Property<int?>("NbLignes")
                        .HasColumnType("int");

                    b.Property<long?>("NoGroupe")
                        .HasColumnType("bigint");

                    b.Property<int>("SiteRno")
                        .HasColumnType("int");

                    b.Property<string>("SiteUid")
                        .IsRequired()
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<decimal?>("Total")
                        .HasColumnType("decimal(7,2)");

                    b.HasKey("Uid", "Rno", "No", "Type");

                    b.ToTable("Docs");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Invitation", b =>
                {
                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RnoClient")
                        .HasColumnType("int");

                    b.Property<string>("UidClient")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.HasKey("Email", "Uid", "Rno");

                    b.ToTable("Invitation");
                });

            modelBuilder.Entity("KalosfideAPI.Data.LigneCLF", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<string>("Uid2")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno2")
                        .HasColumnType("int");

                    b.Property<long>("No2")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal?>("AFixer")
                        .HasColumnType("decimal(8,3)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("Quantité")
                        .HasColumnType("decimal(8,3)");

                    b.Property<string>("TypeCommande")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.HasKey("Uid", "Rno", "No", "Uid2", "Rno2", "No2", "Type");

                    b.HasIndex("Uid", "Rno", "No", "Type");

                    b.HasIndex("Uid2", "Rno2", "No2", "Date");

                    b.ToTable("Lignes");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Produit", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<long>("No")
                        .HasColumnType("bigint");

                    b.Property<long>("CategorieNo")
                        .HasColumnType("bigint");

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1)
                        .HasDefaultValue("D");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<decimal>("Prix")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(7,2)")
                        .HasDefaultValue(0m);

                    b.Property<string>("TypeCommande")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1)
                        .HasDefaultValue("1");

                    b.Property<string>("TypeMesure")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
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
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1)
                        .HasDefaultValue("N");

                    b.Property<int>("SiteRno")
                        .HasColumnType("int");

                    b.Property<string>("SiteUid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("SiteUid", "SiteRno");

                    b.HasIndex("Uid", "Rno");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Site", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<int>("Rno")
                        .HasColumnType("int");

                    b.Property<string>("Adresse")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1)
                        .HasDefaultValue("O");

                    b.Property<string>("FormatNomFichierCommande")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FormatNomFichierFacture")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FormatNomFichierLivraison")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Titre")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Ville")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Uid", "Rno");

                    b.HasIndex("Nom");

                    b.HasIndex("Url");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Utilisateur", b =>
                {
                    b.Property<string>("Uid")
                        .HasColumnType("nvarchar(20)")
                        .HasMaxLength(20);

                    b.Property<string>("Etat")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1)
                        .HasDefaultValue("N");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Uid");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasFilter("[UserId] IS NOT NULL");

                    b.ToTable("Utilisateurs");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(256)")
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
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("KalosfideAPI.Data.Administrateur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithOne()
                        .HasForeignKey("KalosfideAPI.Data.Administrateur", "Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveCatégorie", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Catégorie", "Catégorie")
                        .WithMany("ArchiveCatégories")
                        .HasForeignKey("Uid", "Rno", "No")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveProduit", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Produit", "Produit")
                        .WithMany("ArchiveProduits")
                        .HasForeignKey("Uid", "Rno", "No")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveRole", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithMany("Archives")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.ArchiveUtilisateur", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Utilisateur", "Utilisateur")
                        .WithMany("Etats")
                        .HasForeignKey("Uid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.Catégorie", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Site", "Site")
                        .WithMany("Catégories")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.Client", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Role", "Role")
                        .WithOne()
                        .HasForeignKey("KalosfideAPI.Data.Client", "Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.DocCLF", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Client", "Client")
                        .WithMany("Docs")
                        .HasForeignKey("Uid", "Rno")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.LigneCLF", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Produit", "Produit")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid2", "Rno2", "No2")
                        .IsRequired();

                    b.HasOne("KalosfideAPI.Data.DocCLF", "Doc")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid", "Rno", "No", "Type")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KalosfideAPI.Data.ArchiveProduit", "ArchiveProduit")
                        .WithMany("Lignes")
                        .HasForeignKey("Uid2", "Rno2", "No2", "Date")
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.Produit", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Catégorie", "Catégorie")
                        .WithMany("Produits")
                        .HasForeignKey("Uid", "Rno", "CategorieNo")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KalosfideAPI.Data.Role", b =>
                {
                    b.HasOne("KalosfideAPI.Data.Utilisateur", "Utilisateur")
                        .WithMany("Roles")
                        .HasForeignKey("Uid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KalosfideAPI.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("KalosfideAPI.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
