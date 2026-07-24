using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddAlisIrsaliyesiNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IrsaliyeNo",
                table: "AlisIrsaliyeleri",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IrsaliyeNo",
                table: "AlisIrsaliyeleri");
        }
    }
}
