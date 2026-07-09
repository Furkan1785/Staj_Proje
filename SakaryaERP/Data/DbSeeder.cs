using Microsoft.Extensions.DependencyInjection;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Data;

// Sadece local development kolaylığı içindir (Program.cs'te yalnızca
// IsDevelopment() iken çağrılır). Gün 29'daki gerçek/kapsamlı demo seed
// (5-10 cari, 20-30 malzeme, örnek belgeler) bundan ayrı ve daha büyük olacak.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var cariService = services.GetRequiredService<ICariService>();
        if ((await cariService.GetAllAsync()).Any())
            return;

        var cari1 = await cariService.CreateAsync(new Cari { CariKodu = "C001", Unvan = "Akcan Ticaret Ltd. Şti.", CariTipi = CariTipi.Musteri, VergiNo = "1234567890", Adres = "İstanbul, Kadıköy", Telefon = "02161234567", EMail = "info@akcanticaret.com", KrediLimiti = 50000 });
        var cari2 = await cariService.CreateAsync(new Cari { CariKodu = "C002", Unvan = "Sakarya Metal A.Ş.", CariTipi = CariTipi.Tedarikci, VergiNo = "9876543210", Adres = "Sakarya, Serdivan", Telefon = "02642345678", EMail = "info@sakaryametal.com", KrediLimiti = 100000 });
        await cariService.CreateAsync(new Cari { CariKodu = "C003", Unvan = "Boru Sanayi ve Ticaret", CariTipi = CariTipi.HerIkisi, VergiNo = "5647382910", Adres = "Kocaeli, Gebze", Telefon = "02623456789", EMail = "iletisim@borusanayi.com", KrediLimiti = 75000 });
        await cariService.CreateAsync(new Cari { CariKodu = "C004", Unvan = "Yıldız Otomotiv Ltd.", CariTipi = CariTipi.Musteri, VergiNo = "1122334455", Adres = "Bursa, Nilüfer", Telefon = "02243456789", EMail = "info@yildizoto.com", KrediLimiti = 30000 });
        await cariService.CreateAsync(new Cari { CariKodu = "C005", Unvan = "Ege Kimya San. Tic.", CariTipi = CariTipi.Tedarikci, VergiNo = "6677889900", Adres = "İzmir, Bornova", Telefon = "02323456789", EMail = "info@egekimya.com", KrediLimiti = 60000 });

        var bankaService = services.GetRequiredService<IBankaHesabiService>();
        var banka1 = await bankaService.CreateAsync(new BankaHesabi { HesapAdi = "Şirket Vadesiz Hesap", BankaAdi = "Garanti BBVA", IBAN = "TR120006200023400001234567", ParaBirimi = "TRY" });
        await bankaService.CreateAsync(new BankaHesabi { HesapAdi = "Döviz Hesabı", BankaAdi = "İş Bankası", IBAN = "TR450006400000112345678901", ParaBirimi = "USD" });

        var kasaService = services.GetRequiredService<IKasaHesabiService>();
        var kasa1 = await kasaService.CreateAsync(new KasaHesabi { KasaAdi = "Merkez Kasa", ParaBirimi = "TRY" });
        await kasaService.CreateAsync(new KasaHesabi { KasaAdi = "Şube Kasa", ParaBirimi = "TRY" });

        var subeService = services.GetRequiredService<ISubeService>();
        await subeService.CreateAsync(new Sube { SubeAdi = "Merkez Şube", Adres = "İstanbul, Kadıköy" });
        await subeService.CreateAsync(new Sube { SubeAdi = "Sakarya Şube", Adres = "Sakarya, Serdivan" });
        await subeService.CreateAsync(new Sube { SubeAdi = "Bursa Şube", Adres = "Bursa, Nilüfer" });

        var cariFisiService = services.GetRequiredService<ICariFisiService>();
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-10), FisTipi = FisTipi.Borc, Tutar = 15000, OdemeYontemi = OdemeYontemi.Havale, BankaHesabiId = banka1.Id, Aciklama = "Satış faturası borçlandırma" });
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-3), FisTipi = FisTipi.Alacak, Tutar = 5000, OdemeYontemi = OdemeYontemi.Havale, BankaHesabiId = banka1.Id, Aciklama = "Kısmi tahsilat" });
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari2.Id, Tarih = DateTime.Today.AddDays(-5), FisTipi = FisTipi.Borc, Tutar = 8000, OdemeYontemi = OdemeYontemi.Nakit, KasaHesabiId = kasa1.Id, Aciklama = "Nakit ödeme" });

        var cekSenetService = services.GetRequiredService<ICekSenetService>();
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Cek, BelgeNo = "0123456", CariId = cari1.Id, VadeTarihi = DateTime.Today.AddDays(-4), Tutar = 12000, BankaAdi = "Garanti BBVA", SubeAdi = "Kadıköy Şubesi" });
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Senet, BelgeNo = "S-2026-014", CariId = cari2.Id, VadeTarihi = DateTime.Today, Tutar = 6500, BankaAdi = null, SubeAdi = null });
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Cek, BelgeNo = "0123789", CariId = cari2.Id, VadeTarihi = DateTime.Today.AddDays(20), Tutar = 9800, BankaAdi = "İş Bankası", SubeAdi = "Serdivan Şubesi" });
    }
}
