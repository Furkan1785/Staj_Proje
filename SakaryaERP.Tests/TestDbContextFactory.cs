using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;

namespace SakaryaERP.Tests;

// Her test kendi izole InMemory veritabanını alır (Guid ile benzersiz isim),
// gerçek Postgres yerine — servis katmanındaki iş kurallarını test etmek için
// repository/UnitOfWork mock'lamaya gerek yok, gerçek UnitOfWork + AppDbContext
// kullanılabiliyor.
public static class TestDbContextFactory
{
    public static AppDbContext OlusturYeniBaglam()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
