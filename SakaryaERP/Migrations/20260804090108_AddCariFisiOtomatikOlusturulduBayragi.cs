using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddCariFisiOtomatikOlusturulduBayragi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OtomatikOlusturuldu",
                table: "CariFisleri",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtomatikOlusturuldu",
                table: "CariFisleri");
        }
    }
}
