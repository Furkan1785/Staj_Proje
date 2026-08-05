using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexesToBelgeNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SevkIrsaliyeleri_IrsaliyeNo",
                table: "SevkIrsaliyeleri",
                column: "IrsaliyeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifleri_TeklifNo",
                table: "SatisTeklifleri",
                column: "TeklifNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisleri_SiparisNo",
                table: "SatisSiparisleri",
                column: "SiparisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_FaturaNo",
                table: "SatisFaturalari",
                column: "FaturaNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriTalepleri_TalepNo",
                table: "MusteriTalepleri",
                column: "TalepNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeHareketFisleri_FisNo",
                table: "MalzemeHareketFisleri",
                column: "FisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisleri_SiparisNo",
                table: "AlisSiparisleri",
                column: "SiparisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyeleri_IrsaliyeNo",
                table: "AlisIrsaliyeleri",
                column: "IrsaliyeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_FaturaNo",
                table: "AlisFaturalari",
                column: "FaturaNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SevkIrsaliyeleri_IrsaliyeNo",
                table: "SevkIrsaliyeleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisTeklifleri_TeklifNo",
                table: "SatisTeklifleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisSiparisleri_SiparisNo",
                table: "SatisSiparisleri");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturalari_FaturaNo",
                table: "SatisFaturalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriTalepleri_TalepNo",
                table: "MusteriTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_MalzemeHareketFisleri_FisNo",
                table: "MalzemeHareketFisleri");

            migrationBuilder.DropIndex(
                name: "IX_AlisSiparisleri_SiparisNo",
                table: "AlisSiparisleri");

            migrationBuilder.DropIndex(
                name: "IX_AlisIrsaliyeleri_IrsaliyeNo",
                table: "AlisIrsaliyeleri");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturalari_FaturaNo",
                table: "AlisFaturalari");
        }
    }
}
