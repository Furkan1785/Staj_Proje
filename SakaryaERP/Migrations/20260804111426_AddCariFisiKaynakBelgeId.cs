using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddCariFisiKaynakBelgeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlisFaturasiId",
                table: "CariFisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CekSenetId",
                table: "CariFisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SatisFaturasiId",
                table: "CariFisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_AlisFaturasiId",
                table: "CariFisleri",
                column: "AlisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_CekSenetId",
                table: "CariFisleri",
                column: "CekSenetId");

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_SatisFaturasiId",
                table: "CariFisleri",
                column: "SatisFaturasiId");

            migrationBuilder.AddForeignKey(
                name: "FK_CariFisleri_AlisFaturalari_AlisFaturasiId",
                table: "CariFisleri",
                column: "AlisFaturasiId",
                principalTable: "AlisFaturalari",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CariFisleri_CekSenetler_CekSenetId",
                table: "CariFisleri",
                column: "CekSenetId",
                principalTable: "CekSenetler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CariFisleri_SatisFaturalari_SatisFaturasiId",
                table: "CariFisleri",
                column: "SatisFaturasiId",
                principalTable: "SatisFaturalari",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariFisleri_AlisFaturalari_AlisFaturasiId",
                table: "CariFisleri");

            migrationBuilder.DropForeignKey(
                name: "FK_CariFisleri_CekSenetler_CekSenetId",
                table: "CariFisleri");

            migrationBuilder.DropForeignKey(
                name: "FK_CariFisleri_SatisFaturalari_SatisFaturasiId",
                table: "CariFisleri");

            migrationBuilder.DropIndex(
                name: "IX_CariFisleri_AlisFaturasiId",
                table: "CariFisleri");

            migrationBuilder.DropIndex(
                name: "IX_CariFisleri_CekSenetId",
                table: "CariFisleri");

            migrationBuilder.DropIndex(
                name: "IX_CariFisleri_SatisFaturasiId",
                table: "CariFisleri");

            migrationBuilder.DropColumn(
                name: "AlisFaturasiId",
                table: "CariFisleri");

            migrationBuilder.DropColumn(
                name: "CekSenetId",
                table: "CariFisleri");

            migrationBuilder.DropColumn(
                name: "SatisFaturasiId",
                table: "CariFisleri");
        }
    }
}
