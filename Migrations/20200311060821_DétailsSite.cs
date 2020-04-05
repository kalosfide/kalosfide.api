using Microsoft.EntityFrameworkCore.Migrations;

namespace KalosfideAPI.Migrations
{
    public partial class DétailsSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierCommande",
                table: "Sites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierFacture",
                table: "Sites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierLivraison",
                table: "Sites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ville",
                table: "Sites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierCommande",
                table: "ArchiveSites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierFacture",
                table: "ArchiveSites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatNomFichierLivraison",
                table: "ArchiveSites",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ville",
                table: "ArchiveSites",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveCatégories_Catégories_Uid_Rno_No",
                table: "ArchiveCatégories",
                columns: new[] { "Uid", "Rno", "No" },
                principalTable: "Catégories",
                principalColumns: new[] { "Uid", "Rno", "No" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveProduits_Produits_Uid_Rno_No",
                table: "ArchiveProduits",
                columns: new[] { "Uid", "Rno", "No" },
                principalTable: "Produits",
                principalColumns: new[] { "Uid", "Rno", "No" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveCatégories_Catégories_Uid_Rno_No",
                table: "ArchiveCatégories");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveProduits_Produits_Uid_Rno_No",
                table: "ArchiveProduits");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierCommande",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierFacture",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierLivraison",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Ville",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierCommande",
                table: "ArchiveSites");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierFacture",
                table: "ArchiveSites");

            migrationBuilder.DropColumn(
                name: "FormatNomFichierLivraison",
                table: "ArchiveSites");

            migrationBuilder.DropColumn(
                name: "Ville",
                table: "ArchiveSites");
        }
    }
}
