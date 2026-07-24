using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddAlisFaturasiNoVeIskonto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Iskonto",
                table: "AlisFaturasiKalemleri",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FaturaNo",
                table: "AlisFaturalari",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iskonto",
                table: "AlisFaturasiKalemleri");

            migrationBuilder.DropColumn(
                name: "FaturaNo",
                table: "AlisFaturalari");
        }
    }
}
