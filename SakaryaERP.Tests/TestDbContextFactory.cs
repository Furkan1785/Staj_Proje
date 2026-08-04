using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            // InMemory sağlayıcısı gerçek transaction desteklemiyor; BeginTransactionAsync
            // kullanan servis metotlarını (örn. CariFisiService.IptalEtAsync) test edebilmek
            // için bu uyarı bilinçli olarak yok sayılıyor (transaction no-op olur, sonuç durumu
            // yine de doğru test edilir).
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}
