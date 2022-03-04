using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KalosfideAPI.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Etat = table.Column<int>(nullable: false, defaultValue: 1),
                    SessionId = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveUtilisateurs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Etat = table.Column<int>(nullable: true),
                    IdDernierSite = table.Column<long>(nullable: true),
                    SessionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveUtilisateurs", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveUtilisateurs_Utilisateur_Id",
                        column: x => x.Id,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Utilisateur_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Utilisateur_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Utilisateur_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Utilisateur_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseur",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Siret = table.Column<string>(nullable: true),
                    Etat = table.Column<int>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    UtilisateurId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fournisseur_Utilisateur_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveFournisseur",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Etat = table.Column<int>(nullable: true),
                    Nom = table.Column<string>(maxLength: 200, nullable: true),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    Siret = table.Column<string>(nullable: true),
                    FormatNomFichierCommande = table.Column<string>(nullable: true),
                    FormatNomFichierLivraison = table.Column<string>(nullable: true),
                    FormatNomFichierFacture = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveFournisseur", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveFournisseur_Fournisseur_Id",
                        column: x => x.Id,
                        principalTable: "Fournisseur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandesSite",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Envoi = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesSite", x => x.Email);
                    table.ForeignKey(
                        name: "FK_DemandesSite_Fournisseur_Id",
                        column: x => x.Id,
                        principalTable: "Fournisseur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Url = table.Column<string>(maxLength: 200, nullable: true),
                    Titre = table.Column<string>(maxLength: 200, nullable: true),
                    Ouvert = table.Column<bool>(nullable: false, defaultValue: false),
                    DateCatalogue = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Fournisseur_Id",
                        column: x => x.Id,
                        principalTable: "Fournisseur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveSites",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(maxLength: 200, nullable: true),
                    Titre = table.Column<string>(maxLength: 200, nullable: true),
                    Ouvert = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveSites", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveSites_Sites_Id",
                        column: x => x.Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catégories",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    SiteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catégories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catégories_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Etat = table.Column<int>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    UtilisateurId = table.Column<string>(nullable: true),
                    SiteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Client_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Client_Utilisateur_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveCatégories",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveCatégories", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveCatégories_Catégories_Id",
                        column: x => x.Id,
                        principalTable: "Catégories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    CategorieId = table.Column<long>(nullable: false),
                    TypeMesure = table.Column<int>(nullable: false, defaultValue: 1),
                    TypeCommande = table.Column<int>(nullable: false, defaultValue: 1),
                    Prix = table.Column<decimal>(type: "decimal(7,2)", nullable: false, defaultValue: 0m),
                    Disponible = table.Column<bool>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    SiteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produits_Catégories_CategorieId",
                        column: x => x.CategorieId,
                        principalTable: "Catégories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Produits_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArchiveClient",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Etat = table.Column<int>(maxLength: 1, nullable: true),
                    Nom = table.Column<string>(maxLength: 200, nullable: true),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    FormatNomFichierCommande = table.Column<string>(nullable: true),
                    FormatNomFichierLivraison = table.Column<string>(nullable: true),
                    FormatNomFichierFacture = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveClient", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveClient_Client_Id",
                        column: x => x.Id,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Docs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: true),
                    NoGroupe = table.Column<long>(nullable: true),
                    NbLignes = table.Column<int>(nullable: true),
                    Total = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    Incomplet = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Docs", x => new { x.Id, x.No, x.Type });
                    table.ForeignKey(
                        name: "FK_Docs_Client_Id",
                        column: x => x.Id,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invitation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ClientId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitation", x => new { x.Email, x.Id });
                    table.ForeignKey(
                        name: "FK_Invitation_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitation_Fournisseur_Id",
                        column: x => x.Id,
                        principalTable: "Fournisseur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveProduits",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: true),
                    CategorieId = table.Column<long>(nullable: true),
                    TypeMesure = table.Column<int>(nullable: true),
                    TypeCommande = table.Column<int>(nullable: true),
                    Prix = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    Disponible = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveProduits", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveProduits_Produits_Id",
                        column: x => x.Id,
                        principalTable: "Produits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lignes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    ProduitId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    TypeCommande = table.Column<int>(maxLength: 1, nullable: false),
                    Quantité = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    AFixer = table.Column<decimal>(type: "decimal(8,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lignes", x => new { x.Id, x.No, x.ProduitId, x.Date, x.Type });
                    table.ForeignKey(
                        name: "FK_Lignes_Produits_ProduitId",
                        column: x => x.ProduitId,
                        principalTable: "Produits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lignes_Docs_Id_No_Type",
                        columns: x => new { x.Id, x.No, x.Type },
                        principalTable: "Docs",
                        principalColumns: new[] { "Id", "No", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveCatégories_Id",
                table: "ArchiveCatégories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveClient_Id",
                table: "ArchiveClient",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveFournisseur_Id",
                table: "ArchiveFournisseur",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveProduits_Id",
                table: "ArchiveProduits",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveSites_Id",
                table: "ArchiveSites",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveUtilisateurs_Id",
                table: "ArchiveUtilisateurs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Catégories_SiteId",
                table: "Catégories",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Catégories_Id_Nom",
                table: "Catégories",
                columns: new[] { "Id", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_SiteId",
                table: "Client",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_UtilisateurId",
                table: "Client",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesSite_Id",
                table: "DemandesSite",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseur_UtilisateurId",
                table: "Fournisseur",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitation_ClientId",
                table: "Invitation",
                column: "ClientId",
                unique: true,
                filter: "[ClientId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invitation_Id",
                table: "Invitation",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Lignes_ProduitId",
                table: "Lignes",
                column: "ProduitId");

            migrationBuilder.CreateIndex(
                name: "IX_Lignes_Id_No_Type",
                table: "Lignes",
                columns: new[] { "Id", "No", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Produits_CategorieId",
                table: "Produits",
                column: "CategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_SiteId",
                table: "Produits",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_Id_Nom",
                table: "Produits",
                columns: new[] { "Id", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Url",
                table: "Sites",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Utilisateur",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Utilisateur",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveCatégories");

            migrationBuilder.DropTable(
                name: "ArchiveClient");

            migrationBuilder.DropTable(
                name: "ArchiveFournisseur");

            migrationBuilder.DropTable(
                name: "ArchiveProduits");

            migrationBuilder.DropTable(
                name: "ArchiveSites");

            migrationBuilder.DropTable(
                name: "ArchiveUtilisateurs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DemandesSite");

            migrationBuilder.DropTable(
                name: "Invitation");

            migrationBuilder.DropTable(
                name: "Lignes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Docs");

            migrationBuilder.DropTable(
                name: "Catégories");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "Fournisseur");

            migrationBuilder.DropTable(
                name: "Utilisateur");
        }
    }
}
