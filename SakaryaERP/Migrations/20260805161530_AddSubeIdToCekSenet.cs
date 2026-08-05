using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSubeIdToCekSenet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "CekSenetler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CekSenetler_SubeId",
                table: "CekSenetler",
                column: "SubeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CekSenetler_Subeler_SubeId",
                table: "CekSenetler",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CekSenetler_Subeler_SubeId",
                table: "CekSenetler");

            migrationBuilder.DropIndex(
                name: "IX_CekSenetler_SubeId",
                table: "CekSenetler");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "CekSenetler");
        }
    }
}
