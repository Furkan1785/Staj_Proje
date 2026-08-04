using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedAlisTalebiTeklifi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisSiparisleri_AlisTeklifleri_AlisTeklifiId",
                table: "AlisSiparisleri");

            migrationBuilder.DropTable(
                name: "AlisTeklifiKalemleri");

            migrationBuilder.DropTable(
                name: "AlisTeklifleri");

            migrationBuilder.DropTable(
                name: "AlisTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_AlisSiparisleri_AlisTeklifiId",
                table: "AlisSiparisleri");

            migrationBuilder.DropColumn(
                name: "AlisTeklifiId",
                table: "AlisSiparisleri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlisTeklifiId",
                table: "AlisSiparisleri",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlisTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Icerik = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TalepNo = table.Column<string>(type: "text", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisTalepleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisTeklifleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisTalebiId = table.Column<int>(type: "integer", nullable: true),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisTeklifleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisTeklifleri_AlisTalepleri_AlisTalebiId",
                        column: x => x.AlisTalebiId,
                        principalTable: "AlisTalepleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlisTeklifleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisTeklifiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisTeklifiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Iskonto = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisTeklifiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisTeklifiKalemleri_AlisTeklifleri_AlisTeklifiId",
                        column: x => x.AlisTeklifiId,
                        principalTable: "AlisTeklifleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisTeklifiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisleri_AlisTeklifiId",
                table: "AlisSiparisleri",
                column: "AlisTeklifiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisTalepleri_CariId",
                table: "AlisTalepleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisTeklifiKalemleri_AlisTeklifiId",
                table: "AlisTeklifiKalemleri",
                column: "AlisTeklifiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisTeklifiKalemleri_MalzemeId",
                table: "AlisTeklifiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisTeklifleri_AlisTalebiId",
                table: "AlisTeklifleri",
                column: "AlisTalebiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisTeklifleri_CariId",
                table: "AlisTeklifleri",
                column: "CariId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisSiparisleri_AlisTeklifleri_AlisTeklifiId",
                table: "AlisSiparisleri",
                column: "AlisTeklifiId",
                principalTable: "AlisTeklifleri",
                principalColumn: "Id");
        }
    }
}
