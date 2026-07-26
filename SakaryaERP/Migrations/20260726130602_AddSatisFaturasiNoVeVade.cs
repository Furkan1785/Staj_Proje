using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSatisFaturasiNoVeVade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaturaNo",
                table: "SatisFaturalari",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "VadeTarihi",
                table: "SatisFaturalari",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaturaNo",
                table: "SatisFaturalari");

            migrationBuilder.DropColumn(
                name: "VadeTarihi",
                table: "SatisFaturalari");
        }
    }
}
