using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSiparisTeslimTarihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TeslimTarihi",
                table: "SatisSiparisleri",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeslimTarihi",
                table: "AlisSiparisleri",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeslimTarihi",
                table: "SatisSiparisleri");

            migrationBuilder.DropColumn(
                name: "TeslimTarihi",
                table: "AlisSiparisleri");
        }
    }
}
