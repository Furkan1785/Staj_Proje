using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSubeIdToDocumentsAndCari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "SatisTeklifleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "SatisSiparisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "SatisFaturalari",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "MusteriTalepleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "Cariler",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "CariFisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "AlisFaturalari",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifleri_SubeId",
                table: "SatisTeklifleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisleri_SubeId",
                table: "SatisSiparisleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_SubeId",
                table: "SatisFaturalari",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriTalepleri_SubeId",
                table: "MusteriTalepleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_SubeId",
                table: "Cariler",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_SubeId",
                table: "CariFisleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_SubeId",
                table: "AlisFaturalari",
                column: "SubeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturalari_Subeler_SubeId",
                table: "AlisFaturalari",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CariFisleri_Subeler_SubeId",
                table: "CariFisleri",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cariler_Subeler_SubeId",
                table: "Cariler",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriTalepleri_Subeler_SubeId",
                table: "MusteriTalepleri",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_Subeler_SubeId",
                table: "SatisFaturalari",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisSiparisleri_Subeler_SubeId",
                table: "SatisSiparisleri",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisTeklifleri_Subeler_SubeId",
                table: "SatisTeklifleri",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturalari_Subeler_SubeId",
                table: "AlisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_CariFisleri_Subeler_SubeId",
                table: "CariFisleri");

            migrationBuilder.DropForeignKey(
                name: "FK_Cariler_Subeler_SubeId",
                table: "Cariler");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriTalepleri_Subeler_SubeId",
                table: "MusteriTalepleri");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_Subeler_SubeId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisSiparisleri_Subeler_SubeId",
                table: "SatisSiparisleri");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisTeklifleri_Subeler_SubeId",
                table: "SatisTeklifleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisTeklifleri_SubeId",
                table: "SatisTeklifleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisSiparisleri_SubeId",
                table: "SatisSiparisleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturalari_SubeId",
                table: "SatisFaturalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriTalepleri_SubeId",
                table: "MusteriTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_Cariler_SubeId",
                table: "Cariler");

            migrationBuilder.DropIndex(
                name: "IX_CariFisleri_SubeId",
                table: "CariFisleri");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturalari_SubeId",
                table: "AlisFaturalari");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "SatisTeklifleri");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "SatisSiparisleri");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "SatisFaturalari");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "MusteriTalepleri");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "CariFisleri");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "AlisFaturalari");
        }
    }
}
