using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSatisFaturasiKalemiBirimMaliyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BirimMaliyet",
                table: "SatisFaturasiKalemleri",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            // Geçmiş fatura kalemleri için Malzeme'nin güncel AlisFiyati'yla geriye dönük
            // yaklaşık maliyet ataması (Dashboard'daki Brüt Kar geçmiş dönemler için de
            // anlamlı çıksın diye). Bu tarihten sonra onaylanan faturalarda BirimMaliyet
            // onay anında SatisFaturasiService tarafından gerçek zamanlı yazılır.
            migrationBuilder.Sql(
                """
                UPDATE "SatisFaturasiKalemleri" sfk
                SET "BirimMaliyet" = m."AlisFiyati"
                FROM "Malzemeler" m
                WHERE sfk."MalzemeId" = m."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirimMaliyet",
                table: "SatisFaturasiKalemleri");
        }
    }
}
