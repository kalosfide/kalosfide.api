using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KalosfideAPI.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUser",
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
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveClients",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveClients", x => new { x.Uid, x.Rno, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "ArchiveSites",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(maxLength: 200, nullable: true),
                    Titre = table.Column<string>(maxLength: 200, nullable: true),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    FormatNomFichierCommande = table.Column<string>(nullable: true),
                    FormatNomFichierLivraison = table.Column<string>(nullable: true),
                    FormatNomFichierFacture = table.Column<string>(nullable: true),
                    Etat = table.Column<string>(maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveSites", x => new { x.Uid, x.Rno, x.Date });
                });

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
                name: "Invitation",
                columns: table => new
                {
                    Email = table.Column<string>(maxLength: 20, nullable: false),
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    UidClient = table.Column<string>(maxLength: 20, nullable: true),
                    RnoClient = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitation", x => new { x.Email, x.Uid, x.Rno });
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Url = table.Column<string>(maxLength: 200, nullable: true),
                    Titre = table.Column<string>(maxLength: 200, nullable: true),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true),
                    Ville = table.Column<string>(nullable: true),
                    FormatNomFichierCommande = table.Column<string>(nullable: true),
                    FormatNomFichierLivraison = table.Column<string>(nullable: true),
                    FormatNomFichierFacture = table.Column<string>(nullable: true),
                    Etat = table.Column<string>(maxLength: 1, nullable: true, defaultValue: "O")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => new { x.Uid, x.Rno });
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
                        name: "FK_AspNetUserClaims_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
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
                        name: "FK_AspNetUserLogins_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
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
                        name: "FK_AspNetUserTokens_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Etat = table.Column<string>(maxLength: 1, nullable: true, defaultValue: "N")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_AspNetUserRoles_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catégories",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catégories", x => new { x.Uid, x.Rno, x.No });
                    table.ForeignKey(
                        name: "FK_Catégories_Sites_Uid_Rno",
                        columns: x => new { x.Uid, x.Rno },
                        principalTable: "Sites",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveUtilisateurs",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Etat = table.Column<string>(maxLength: 1, nullable: true),
                    NoDernierRole = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveUtilisateurs", x => new { x.Uid, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveUtilisateurs_Utilisateurs_Uid",
                        column: x => x.Uid,
                        principalTable: "Utilisateurs",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    SiteUid = table.Column<string>(maxLength: 20, nullable: true),
                    SiteRno = table.Column<int>(nullable: false),
                    Etat = table.Column<string>(maxLength: 1, nullable: true, defaultValue: "N")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => new { x.Uid, x.Rno });
                    table.ForeignKey(
                        name: "FK_Roles_Utilisateurs_Uid",
                        column: x => x.Uid,
                        principalTable: "Utilisateurs",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_Sites_SiteUid_SiteRno",
                        columns: x => new { x.SiteUid, x.SiteRno },
                        principalTable: "Sites",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveCatégories",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveCatégories", x => new { x.Uid, x.Rno, x.No, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveCatégories_Catégories_Uid_Rno_No",
                        columns: x => new { x.Uid, x.Rno, x.No },
                        principalTable: "Catégories",
                        principalColumns: new[] { "Uid", "Rno", "No" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    CategorieNo = table.Column<long>(nullable: false),
                    TypeMesure = table.Column<string>(maxLength: 1, nullable: false, defaultValue: "U"),
                    TypeCommande = table.Column<string>(maxLength: 1, nullable: false, defaultValue: "1"),
                    Prix = table.Column<decimal>(type: "decimal(7,2)", nullable: false, defaultValue: 0m),
                    Etat = table.Column<string>(maxLength: 1, nullable: true, defaultValue: "D")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => new { x.Uid, x.Rno, x.No });
                    table.ForeignKey(
                        name: "FK_Produits_Catégories_Uid_Rno_CategorieNo",
                        columns: x => new { x.Uid, x.Rno, x.CategorieNo },
                        principalTable: "Catégories",
                        principalColumns: new[] { "Uid", "Rno", "No" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Administrateurs",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrateurs", x => new { x.Uid, x.Rno });
                    table.ForeignKey(
                        name: "FK_Administrateurs_Roles_Uid_Rno",
                        columns: x => new { x.Uid, x.Rno },
                        principalTable: "Roles",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRoles",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Etat = table.Column<string>(maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRoles", x => new { x.Uid, x.Rno, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveRoles_Roles_Uid_Rno",
                        columns: x => new { x.Uid, x.Rno },
                        principalTable: "Roles",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => new { x.Uid, x.Rno });
                    table.ForeignKey(
                        name: "FK_Clients_Roles_Uid_Rno",
                        columns: x => new { x.Uid, x.Rno },
                        principalTable: "Roles",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveProduits",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: true),
                    CategorieNo = table.Column<long>(nullable: true),
                    TypeMesure = table.Column<string>(maxLength: 1, nullable: true),
                    TypeCommande = table.Column<string>(maxLength: 1, nullable: true),
                    Prix = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    Etat = table.Column<string>(maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveProduits", x => new { x.Uid, x.Rno, x.No, x.Date });
                    table.ForeignKey(
                        name: "FK_ArchiveProduits_Produits_Uid_Rno_No",
                        columns: x => new { x.Uid, x.Rno, x.No },
                        principalTable: "Produits",
                        principalColumns: new[] { "Uid", "Rno", "No" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Docs",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: true),
                    NoGroupe = table.Column<long>(nullable: true),
                    SiteUid = table.Column<string>(maxLength: 20, nullable: false),
                    SiteRno = table.Column<int>(nullable: false),
                    NbLignes = table.Column<int>(nullable: true),
                    Total = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    Incomplet = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Docs", x => new { x.Uid, x.Rno, x.No, x.Type });
                    table.ForeignKey(
                        name: "FK_Docs_Clients_Uid_Rno",
                        columns: x => new { x.Uid, x.Rno },
                        principalTable: "Clients",
                        principalColumns: new[] { "Uid", "Rno" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lignes",
                columns: table => new
                {
                    Uid = table.Column<string>(maxLength: 20, nullable: false),
                    Rno = table.Column<int>(nullable: false),
                    No = table.Column<long>(nullable: false),
                    Uid2 = table.Column<string>(maxLength: 20, nullable: false),
                    Rno2 = table.Column<int>(nullable: false),
                    No2 = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    TypeCommande = table.Column<string>(maxLength: 1, nullable: true),
                    Quantité = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    AFixer = table.Column<decimal>(type: "decimal(8,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lignes", x => new { x.Uid, x.Rno, x.No, x.Uid2, x.Rno2, x.No2, x.Type });
                    table.ForeignKey(
                        name: "FK_Lignes_Produits_Uid2_Rno2_No2",
                        columns: x => new { x.Uid2, x.Rno2, x.No2 },
                        principalTable: "Produits",
                        principalColumns: new[] { "Uid", "Rno", "No" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lignes_Docs_Uid_Rno_No_Type",
                        columns: x => new { x.Uid, x.Rno, x.No, x.Type },
                        principalTable: "Docs",
                        principalColumns: new[] { "Uid", "Rno", "No", "Type" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lignes_ArchiveProduits_Uid2_Rno2_No2_Date",
                        columns: x => new { x.Uid2, x.Rno2, x.No2, x.Date },
                        principalTable: "ArchiveProduits",
                        principalColumns: new[] { "Uid", "Rno", "No", "Date" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "ApplicationUser",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "ApplicationUser",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveCatégories_Uid_Rno_No",
                table: "ArchiveCatégories",
                columns: new[] { "Uid", "Rno", "No" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveClients_Uid_Rno",
                table: "ArchiveClients",
                columns: new[] { "Uid", "Rno" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveProduits_Uid_Rno_No",
                table: "ArchiveProduits",
                columns: new[] { "Uid", "Rno", "No" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRoles_Uid_Rno",
                table: "ArchiveRoles",
                columns: new[] { "Uid", "Rno" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveSites_Uid_Rno",
                table: "ArchiveSites",
                columns: new[] { "Uid", "Rno" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveUtilisateurs_Uid",
                table: "ArchiveUtilisateurs",
                column: "Uid");

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
                name: "IX_Catégories_Uid_Rno_Nom",
                table: "Catégories",
                columns: new[] { "Uid", "Rno", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Nom",
                table: "Clients",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Lignes_Uid_Rno_No_Type",
                table: "Lignes",
                columns: new[] { "Uid", "Rno", "No", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Lignes_Uid2_Rno2_No2_Date",
                table: "Lignes",
                columns: new[] { "Uid2", "Rno2", "No2", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Produits_Uid_Rno_CategorieNo",
                table: "Produits",
                columns: new[] { "Uid", "Rno", "CategorieNo" });

            migrationBuilder.CreateIndex(
                name: "IX_Produits_Uid_Rno_Nom",
                table: "Produits",
                columns: new[] { "Uid", "Rno", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_SiteUid_SiteRno",
                table: "Roles",
                columns: new[] { "SiteUid", "SiteRno" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Uid_Rno",
                table: "Roles",
                columns: new[] { "Uid", "Rno" });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Nom",
                table: "Sites",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Url",
                table: "Sites",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_UserId",
                table: "Utilisateurs",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrateurs");

            migrationBuilder.DropTable(
                name: "ArchiveCatégories");

            migrationBuilder.DropTable(
                name: "ArchiveClients");

            migrationBuilder.DropTable(
                name: "ArchiveRoles");

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
                name: "Invitation");

            migrationBuilder.DropTable(
                name: "Lignes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Docs");

            migrationBuilder.DropTable(
                name: "ArchiveProduits");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Catégories");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "ApplicationUser");
        }
    }
}
