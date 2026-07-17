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
        var subeMerkez = await subeService.CreateAsync(new Sube { SubeAdi = "Merkez Şube", Adres = "İstanbul, Kadıköy" });
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

        var malzemeKategoriService = services.GetRequiredService<IMalzemeKategoriService>();
        var metalUrunler = await malzemeKategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Metal Ürünler" });
        var boruProfil = await malzemeKategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Boru ve Profil", ParentId = metalUrunler.Id });
        var sacUrunleri = await malzemeKategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Sac Ürünleri", ParentId = metalUrunler.Id });
        var kimyasalUrunler = await malzemeKategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Kimyasal Ürünler" });
        var boyaKaplama = await malzemeKategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Boya ve Kaplama", ParentId = kimyasalUrunler.Id });

        var malzemeService = services.GetRequiredService<IMalzemeService>();
        var m001 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M001", Barkod = "8690000000011", MalzemeAdi = "Siyah Boru 1/2 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Siyah", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 45, SatisFiyati = 62, KdvOrani = 20, MinStokMiktari = 500, MaxStokMiktari = 5000, RafNo = "A-01", Bakiye = 1200, KategoriId = boruProfil.Id });
        var m002 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M002", Barkod = "8690000000028", MalzemeAdi = "Galvaniz Boru 3/4 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Galvaniz", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 58, SatisFiyati = 79, KdvOrani = 20, MinStokMiktari = 300, MaxStokMiktari = 4000, RafNo = "A-02", Bakiye = 850, KategoriId = boruProfil.Id });
        var m003 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M003", Barkod = "8690000000035", MalzemeAdi = "Soğuk Çekme Profil 40x40", Marka = "Boru Sanayi", Kalite = "St52", Tip = "Kare Profil", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.Hammadde, AlisFiyati = 72, SatisFiyati = 95, KdvOrani = 20, MinStokMiktari = 200, MaxStokMiktari = 3000, RafNo = "A-03", Bakiye = 430, KategoriId = boruProfil.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M004", Barkod = "8690000000042", MalzemeAdi = "Sıcak Haddelenmiş Sac 3mm", Marka = "Boru Sanayi", Kalite = "St37", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.Hammadde, AlisFiyati = 210, SatisFiyati = 260, KdvOrani = 20, MinStokMiktari = 100, MaxStokMiktari = 1500, RafNo = "B-01", Bakiye = 340, KategoriId = sacUrunleri.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M005", Barkod = "8690000000059", MalzemeAdi = "Galvaniz Sac 1mm", Marka = "Ege Kimya", Kalite = "DX51D", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 95, SatisFiyati = 128, KdvOrani = 20, MinStokMiktari = 150, MaxStokMiktari = 2000, RafNo = "B-02", Bakiye = 610, KategoriId = sacUrunleri.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M006", Barkod = "8690000000066", MalzemeAdi = "Epoksi Boya - Gri", Marka = "Ege Kimya", Kalite = "Endüstriyel", Tip = "Sıvı", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 85, SatisFiyati = 115, KdvOrani = 20, MinStokMiktari = 50, MaxStokMiktari = 800, RafNo = "C-01", Bakiye = 220, KategoriId = boyaKaplama.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M007", Barkod = "8690000000073", MalzemeAdi = "Astar Boya", Marka = "Ege Kimya", Kalite = "Endüstriyel", Tip = "Sıvı", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 60, SatisFiyati = 82, KdvOrani = 20, MinStokMiktari = 40, MaxStokMiktari = 600, RafNo = "C-02", Bakiye = 90, KategoriId = boyaKaplama.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M008", MalzemeAdi = "Kaynak Teli 1.2mm", Marka = "Yıldız Otomotiv", Kalite = "Çelik", Tip = "Bobin", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 130, SatisFiyati = 175, KdvOrani = 20, MinStokMiktari = 20, MaxStokMiktari = 300, RafNo = "D-01", Bakiye = 45 });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M009", MalzemeAdi = "Kaynaklı Boru Yarı Mamul 2 inç", Birim = "Metre", TeminTuru = TeminTuru.Uretim, StokTipi = StokTipi.YariMamul, AlisFiyati = 0, SatisFiyati = 0, KdvOrani = 20, MinStokMiktari = 100, MaxStokMiktari = 1000, RafNo = "E-01", Bakiye = 300, KategoriId = boruProfil.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M010", MalzemeAdi = "Bitmiş Ürün - Korkuluk Sistemi", Birim = "Adet", TeminTuru = TeminTuru.Uretim, StokTipi = StokTipi.Mamul, AlisFiyati = 0, SatisFiyati = 850, KdvOrani = 20, MinStokMiktari = 5, MaxStokMiktari = 50, RafNo = "F-01", Bakiye = 12 });

        // Durumu hâlâ Beklemede — bakiye güncellemesi ve stok kontrolü Gün 14'ün işi.
        var malzemeHareketFisiService = services.GetRequiredService<IMalzemeHareketFisiService>();
        await malzemeHareketFisiService.CreateAsync(
            new MalzemeHareketFisi { Tarih = DateTime.Today.AddDays(-2), HareketTipi = HareketTipi.Giris, SubeId = subeMerkez.Id },
            [
                new MalzemeHareketFisiKalemi { MalzemeId = m001.Id, Miktar = 500, Aciklama = "Tedarikçiden gelen sevkiyat" },
                new MalzemeHareketFisiKalemi { MalzemeId = m002.Id, Miktar = 300, Aciklama = "Tedarikçiden gelen sevkiyat" }
            ]);
        await malzemeHareketFisiService.CreateAsync(
            new MalzemeHareketFisi { Tarih = DateTime.Today, HareketTipi = HareketTipi.Cikis, SubeId = subeMerkez.Id },
            [
                new MalzemeHareketFisiKalemi { MalzemeId = m003.Id, Miktar = 50, Aciklama = "Müşteri siparişi sevkiyatı" }
            ]);
    }
}
