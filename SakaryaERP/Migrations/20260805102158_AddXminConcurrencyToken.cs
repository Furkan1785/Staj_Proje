using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddXminConcurrencyToken : Migration
    {
        // "xmin" Postgres'in her tabloda zaten var olan bir sistem sütunudur (Add/DropColumn ile
        // dokunulamaz — "column name xmin conflicts with a system column name" hatası verir).
        // Bu migration sadece EF Core model snapshot'ının xmin'i concurrency token olarak
        // tanımasını sağlamak için var; gerçek bir şema değişikliği yapmıyor (bkz. AppDbContext
        // OnModelCreating, Property<uint>("xmin")...IsConcurrencyToken()).

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
