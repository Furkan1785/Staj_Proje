using Microsoft.Extensions.DependencyInjection;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Data;

// Gün 29: Oracle Cloud'a ilk deploy sonrası `dotnet SakaryaERP.dll --seed-demo` ile elle
// tetiklenir (Program.cs). DbSeeder.SeedDevKolayligiAsync'ten ayrı ve daha kapsamlı: 8 cari,
// 25 malzeme, uçtan uca örnek belgeler (talep→teklif→sipariş→irsaliye→fatura ve alış
// siparişi→irsaliye→fatura), akışın farklı aşamalarında bırakılmış belgeler dahil.
// "Demo şirket" ayrı bir tablo değil — aşağıdaki 3 şube tek bir demo şirketin şubelerini temsil eder.
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var cariService = services.GetRequiredService<ICariService>();
        if ((await cariService.GetAllAsync()).Any())
            return;

        var subeService = services.GetRequiredService<ISubeService>();
        var subeMerkez = await subeService.CreateAsync(new Sube { SubeAdi = "Merkez Şube", Adres = "İstanbul, Kadıköy" });
        var subeSakarya = await subeService.CreateAsync(new Sube { SubeAdi = "Sakarya Şube", Adres = "Sakarya, Serdivan" });
        await subeService.CreateAsync(new Sube { SubeAdi = "Bursa Şube", Adres = "Bursa, Nilüfer" });

        var bankaService = services.GetRequiredService<IBankaHesabiService>();
        var banka1 = await bankaService.CreateAsync(new BankaHesabi { HesapAdi = "Şirket Vadesiz Hesap", BankaAdi = "Garanti BBVA", IBAN = "TR120006200023400001234567", ParaBirimi = "TRY" });
        await bankaService.CreateAsync(new BankaHesabi { HesapAdi = "Döviz Hesabı", BankaAdi = "İş Bankası", IBAN = "TR450006400000112345678901", ParaBirimi = "USD" });

        var kasaService = services.GetRequiredService<IKasaHesabiService>();
        var kasa1 = await kasaService.CreateAsync(new KasaHesabi { KasaAdi = "Merkez Kasa", ParaBirimi = "TRY" });
        await kasaService.CreateAsync(new KasaHesabi { KasaAdi = "Şube Kasa", ParaBirimi = "TRY" });

        var cari1 = await cariService.CreateAsync(new Cari { CariKodu = "C001", Unvan = "Akcan Ticaret Ltd. Şti.", CariTipi = CariTipi.Musteri, VergiNo = "1234567890", Adres = "İstanbul, Kadıköy", Telefon = "02161234567", EMail = "info@akcanticaret.com", KrediLimiti = 50000 });
        var cari2 = await cariService.CreateAsync(new Cari { CariKodu = "C002", Unvan = "Sakarya Metal A.Ş.", CariTipi = CariTipi.Tedarikci, VergiNo = "9876543210", Adres = "Sakarya, Serdivan", Telefon = "02642345678", EMail = "info@sakaryametal.com", KrediLimiti = 150000 });
        var cari3 = await cariService.CreateAsync(new Cari { CariKodu = "C003", Unvan = "Boru Sanayi ve Ticaret", CariTipi = CariTipi.HerIkisi, VergiNo = "5647382910", Adres = "Kocaeli, Gebze", Telefon = "02623456789", EMail = "iletisim@borusanayi.com", KrediLimiti = 75000 });
        var cari4 = await cariService.CreateAsync(new Cari { CariKodu = "C004", Unvan = "Yıldız Otomotiv Ltd.", CariTipi = CariTipi.Musteri, VergiNo = "1122334455", Adres = "Bursa, Nilüfer", Telefon = "02243456789", EMail = "info@yildizoto.com", KrediLimiti = 40000 });
        var cari5 = await cariService.CreateAsync(new Cari { CariKodu = "C005", Unvan = "Ege Kimya San. Tic.", CariTipi = CariTipi.Tedarikci, VergiNo = "6677889900", Adres = "İzmir, Bornova", Telefon = "02323456789", EMail = "info@egekimya.com", KrediLimiti = 80000 });
        var cari6 = await cariService.CreateAsync(new Cari { CariKodu = "C006", Unvan = "Marmara Çelik Ltd. Şti.", CariTipi = CariTipi.Tedarikci, VergiNo = "3344556677", Adres = "Kocaeli, İzmit", Telefon = "02623334455", EMail = "info@marmaracelik.com", KrediLimiti = 120000 });
        var cari7 = await cariService.CreateAsync(new Cari { CariKodu = "C007", Unvan = "Anadolu İnşaat Malzemeleri A.Ş.", CariTipi = CariTipi.Musteri, VergiNo = "7788990011", Adres = "Ankara, Çankaya", Telefon = "03123456789", EMail = "info@anadoluinsaat.com", KrediLimiti = 60000 });
        var cari8 = await cariService.CreateAsync(new Cari { CariKodu = "C008", Unvan = "Karadeniz Elektrik San.", CariTipi = CariTipi.HerIkisi, VergiNo = "2233445566", Adres = "Samsun, İlkadım", Telefon = "03623456789", EMail = "info@karadenizelektrik.com", KrediLimiti = 45000 });

        var kategoriService = services.GetRequiredService<IMalzemeKategoriService>();
        var metalUrunler = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Metal Ürünler" });
        var boruProfil = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Boru ve Profil", ParentId = metalUrunler.Id });
        var sacUrunleri = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Sac Ürünleri", ParentId = metalUrunler.Id });
        var kimyasalUrunler = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Kimyasal Ürünler" });
        var boyaKaplama = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Boya ve Kaplama", ParentId = kimyasalUrunler.Id });
        var elektrikMalzemeleri = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Elektrik Malzemeleri" });
        var baglantiElemanlari = await kategoriService.CreateAsync(new MalzemeKategori { KategoriAdi = "Bağlantı Elemanları" });

        var malzemeService = services.GetRequiredService<IMalzemeService>();

        var m01 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M001", Barkod = "8690000000011", MalzemeAdi = "Siyah Boru 1/2 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Siyah", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 45, SatisFiyati = 62, KdvOrani = 20, MinStokMiktari = 500, MaxStokMiktari = 5000, RafNo = "A-01", Bakiye = 1200, KategoriId = boruProfil.Id });
        var m02 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M002", Barkod = "8690000000028", MalzemeAdi = "Galvaniz Boru 3/4 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Galvaniz", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 58, SatisFiyati = 79, KdvOrani = 20, MinStokMiktari = 300, MaxStokMiktari = 4000, RafNo = "A-02", Bakiye = 850, KategoriId = boruProfil.Id });
        var m03 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M003", Barkod = "8690000000035", MalzemeAdi = "Soğuk Çekme Profil 40x40", Marka = "Boru Sanayi", Kalite = "St52", Tip = "Kare Profil", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.Hammadde, AlisFiyati = 72, SatisFiyati = 95, KdvOrani = 20, MinStokMiktari = 200, MaxStokMiktari = 3000, RafNo = "A-03", Bakiye = 430, KategoriId = boruProfil.Id });
        var m04 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M004", Barkod = "8690000000042", MalzemeAdi = "Siyah Boru 1 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Siyah", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 68, SatisFiyati = 92, KdvOrani = 20, MinStokMiktari = 300, MaxStokMiktari = 4000, RafNo = "A-04", Bakiye = 640, KategoriId = boruProfil.Id });
        var m05 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M005", Barkod = "8690000000059", MalzemeAdi = "Galvaniz Boru 1 inç", Marka = "Sakarya Metal", Kalite = "St37", Tip = "Galvaniz", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 81, SatisFiyati = 108, KdvOrani = 20, MinStokMiktari = 200, MaxStokMiktari = 3000, RafNo = "A-05", Bakiye = 390, KategoriId = boruProfil.Id });

        var m06 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M006", Barkod = "8690000000066", MalzemeAdi = "Sıcak Haddelenmiş Sac 3mm", Marka = "Marmara Çelik", Kalite = "St37", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.Hammadde, AlisFiyati = 210, SatisFiyati = 260, KdvOrani = 20, MinStokMiktari = 100, MaxStokMiktari = 1500, RafNo = "B-01", Bakiye = 340, KategoriId = sacUrunleri.Id });
        var m07 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M007", Barkod = "8690000000073", MalzemeAdi = "Galvaniz Sac 1mm", Marka = "Marmara Çelik", Kalite = "DX51D", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 95, SatisFiyati = 128, KdvOrani = 20, MinStokMiktari = 150, MaxStokMiktari = 2000, RafNo = "B-02", Bakiye = 610, KategoriId = sacUrunleri.Id });
        var m08 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M008", Barkod = "8690000000080", MalzemeAdi = "Paslanmaz Sac 2mm", Marka = "Marmara Çelik", Kalite = "304", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 340, SatisFiyati = 425, KdvOrani = 20, MinStokMiktari = 50, MaxStokMiktari = 700, RafNo = "B-03", Bakiye = 160, KategoriId = sacUrunleri.Id });
        var m09 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M009", Barkod = "8690000000097", MalzemeAdi = "Alüminyum Sac 1.5mm", Marka = "Marmara Çelik", Kalite = "5754", Tip = "Sac", Birim = "m2", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 190, SatisFiyati = 245, KdvOrani = 20, MinStokMiktari = 80, MaxStokMiktari = 900, RafNo = "B-04", Bakiye = 210, KategoriId = sacUrunleri.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M010", Barkod = "8690000000103", MalzemeAdi = "Sac Profil 50x50", Marka = "Marmara Çelik", Kalite = "St37", Tip = "Profil", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 88, SatisFiyati = 115, KdvOrani = 20, MinStokMiktari = 150, MaxStokMiktari = 2000, RafNo = "B-05", Bakiye = 380, KategoriId = sacUrunleri.Id });

        var m11 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M011", Barkod = "8690000000110", MalzemeAdi = "Epoksi Boya - Gri", Marka = "Ege Kimya", Kalite = "Endüstriyel", Tip = "Sıvı", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 85, SatisFiyati = 115, KdvOrani = 20, MinStokMiktari = 50, MaxStokMiktari = 800, RafNo = "C-01", Bakiye = 220, KategoriId = boyaKaplama.Id });
        var m12 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M012", Barkod = "8690000000127", MalzemeAdi = "Astar Boya", Marka = "Ege Kimya", Kalite = "Endüstriyel", Tip = "Sıvı", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 60, SatisFiyati = 82, KdvOrani = 20, MinStokMiktari = 40, MaxStokMiktari = 600, RafNo = "C-02", Bakiye = 90, KategoriId = boyaKaplama.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M013", Barkod = "8690000000134", MalzemeAdi = "Sentetik Boya - Mavi", Marka = "Ege Kimya", Kalite = "Standart", Tip = "Sıvı", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 70, SatisFiyati = 95, KdvOrani = 20, MinStokMiktari = 30, MaxStokMiktari = 500, RafNo = "C-03", Bakiye = 65, KategoriId = boyaKaplama.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M014", Barkod = "8690000000141", MalzemeAdi = "Toz Boya - Siyah", Marka = "Ege Kimya", Kalite = "Elektrostatik", Tip = "Toz", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 110, SatisFiyati = 145, KdvOrani = 20, MinStokMiktari = 30, MaxStokMiktari = 400, RafNo = "C-04", Bakiye = 55, KategoriId = boyaKaplama.Id });

        var m15 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M015", Barkod = "8690000000158", MalzemeAdi = "Kablo 2.5mm NYA", Marka = "Karadeniz Elektrik", Kalite = "TSE", Tip = "Kablo", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 12, SatisFiyati = 17, KdvOrani = 20, MinStokMiktari = 1000, MaxStokMiktari = 10000, RafNo = "D-01", Bakiye = 3200, KategoriId = elektrikMalzemeleri.Id });
        var m16 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M016", Barkod = "8690000000165", MalzemeAdi = "Sigorta 16A", Marka = "Karadeniz Elektrik", Kalite = "TSE", Tip = "Otomat Sigorta", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 45, SatisFiyati = 62, KdvOrani = 20, MinStokMiktari = 100, MaxStokMiktari = 1000, RafNo = "D-02", Bakiye = 340, KategoriId = elektrikMalzemeleri.Id });
        var m17 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M017", Barkod = "8690000000172", MalzemeAdi = "Priz Grubu", Marka = "Karadeniz Elektrik", Kalite = "Standart", Tip = "Priz", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 35, SatisFiyati = 48, KdvOrani = 20, MinStokMiktari = 80, MaxStokMiktari = 900, RafNo = "D-03", Bakiye = 210, KategoriId = elektrikMalzemeleri.Id });
        var m18 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M018", Barkod = "8690000000189", MalzemeAdi = "Aydınlatma Armatürü LED", Marka = "Karadeniz Elektrik", Kalite = "Standart", Tip = "Armatür", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 180, SatisFiyati = 245, KdvOrani = 20, MinStokMiktari = 30, MaxStokMiktari = 400, RafNo = "D-04", Bakiye = 95, KategoriId = elektrikMalzemeleri.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M019", Barkod = "8690000000196", MalzemeAdi = "Kablo Kanalı", Marka = "Karadeniz Elektrik", Kalite = "PVC", Tip = "Kanal", Birim = "Metre", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 22, SatisFiyati = 31, KdvOrani = 20, MinStokMiktari = 200, MaxStokMiktari = 2500, RafNo = "D-05", Bakiye = 480, KategoriId = elektrikMalzemeleri.Id });

        var m20 = await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M020", Barkod = "8690000000202", MalzemeAdi = "Cıvata M8x40", Marka = "Anadolu İnşaat", Kalite = "8.8", Tip = "Cıvata", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 3, SatisFiyati = 5, KdvOrani = 20, MinStokMiktari = 2000, MaxStokMiktari = 20000, RafNo = "E-01", Bakiye = 6400, KategoriId = baglantiElemanlari.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M021", Barkod = "8690000000219", MalzemeAdi = "Somun M8", Marka = "Anadolu İnşaat", Kalite = "8.8", Tip = "Somun", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 1.5m, SatisFiyati = 2.5m, KdvOrani = 20, MinStokMiktari = 2000, MaxStokMiktari = 20000, RafNo = "E-02", Bakiye = 7200, KategoriId = baglantiElemanlari.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M022", Barkod = "8690000000226", MalzemeAdi = "Rondela M8", Marka = "Anadolu İnşaat", Kalite = "Standart", Tip = "Rondela", Birim = "Adet", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 0.8m, SatisFiyati = 1.4m, KdvOrani = 20, MinStokMiktari = 3000, MaxStokMiktari = 30000, RafNo = "E-03", Bakiye = 9500, KategoriId = baglantiElemanlari.Id });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M023", Barkod = "8690000000233", MalzemeAdi = "Kaynak Elektrodu 3.2mm", Marka = "Yıldız Otomotiv", Kalite = "Çelik", Tip = "Elektrod", Birim = "Kg", TeminTuru = TeminTuru.Alis, StokTipi = StokTipi.TicariMal, AlisFiyati = 130, SatisFiyati = 175, KdvOrani = 20, MinStokMiktari = 20, MaxStokMiktari = 300, RafNo = "E-04", Bakiye = 45, KategoriId = baglantiElemanlari.Id });

        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M024", MalzemeAdi = "Kaynaklı Boru Yarı Mamul 2 inç", Birim = "Metre", TeminTuru = TeminTuru.Uretim, StokTipi = StokTipi.YariMamul, AlisFiyati = 0, SatisFiyati = 0, KdvOrani = 20, MinStokMiktari = 100, MaxStokMiktari = 1000, RafNo = "F-01", Bakiye = 300 });
        await malzemeService.CreateAsync(new Malzeme { MalzemeKodu = "M025", MalzemeAdi = "Bitmiş Ürün - Korkuluk Sistemi", Birim = "Adet", TeminTuru = TeminTuru.Uretim, StokTipi = StokTipi.Mamul, AlisFiyati = 0, SatisFiyati = 850, KdvOrani = 20, MinStokMiktari = 5, MaxStokMiktari = 50, RafNo = "F-02", Bakiye = 12 });

        var cariFisiService = services.GetRequiredService<ICariFisiService>();
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-20), FisTipi = FisTipi.Borc, Tutar = 15000, OdemeYontemi = OdemeYontemi.Havale, BankaHesabiId = banka1.Id, Aciklama = "Satış faturası borçlandırma" });
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-13), FisTipi = FisTipi.Alacak, Tutar = 5000, OdemeYontemi = OdemeYontemi.Havale, BankaHesabiId = banka1.Id, Aciklama = "Kısmi tahsilat" });
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari2.Id, Tarih = DateTime.Today.AddDays(-15), FisTipi = FisTipi.Borc, Tutar = 8000, OdemeYontemi = OdemeYontemi.Nakit, KasaHesabiId = kasa1.Id, Aciklama = "Nakit ödeme" });
        await cariFisiService.CreateAsync(new CariFisi { CariId = cari7.Id, Tarih = DateTime.Today.AddDays(-8), FisTipi = FisTipi.Alacak, Tutar = 3000, OdemeYontemi = OdemeYontemi.Havale, BankaHesabiId = banka1.Id, Aciklama = "Kısmi tahsilat" });

        var cekSenetService = services.GetRequiredService<ICekSenetService>();
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Cek, BelgeNo = "0123456", CariId = cari1.Id, VadeTarihi = DateTime.Today.AddDays(-4), Tutar = 12000, BankaAdi = "Garanti BBVA", SubeAdi = "Kadıköy Şubesi" });
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Senet, BelgeNo = "S-2026-014", CariId = cari4.Id, VadeTarihi = DateTime.Today, Tutar = 6500, BankaAdi = null, SubeAdi = null });
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Cek, BelgeNo = "0123789", CariId = cari3.Id, VadeTarihi = DateTime.Today.AddDays(20), Tutar = 9800, BankaAdi = "İş Bankası", SubeAdi = "İzmit Şubesi" });
        await cekSenetService.CreateAsync(new CekSenet { BelgeTipi = BelgeTipi.Senet, BelgeNo = "S-2026-021", CariId = cari7.Id, VadeTarihi = DateTime.Today.AddDays(35), Tutar = 4200, BankaAdi = null, SubeAdi = null });

        var malzemeHareketFisiService = services.GetRequiredService<IMalzemeHareketFisiService>();
        var giris1 = await malzemeHareketFisiService.CreateAsync(
            new MalzemeHareketFisi { Tarih = DateTime.Today.AddDays(-6), HareketTipi = HareketTipi.Giris, SubeId = subeMerkez.Id },
            [
                new MalzemeHareketFisiKalemi { MalzemeId = m01.Id, Miktar = 500, Aciklama = "Tedarikçiden gelen sevkiyat" },
                new MalzemeHareketFisiKalemi { MalzemeId = m02.Id, Miktar = 300, Aciklama = "Tedarikçiden gelen sevkiyat" }
            ]);
        await malzemeHareketFisiService.OnaylaAsync(giris1.Id);

        var cikis1 = await malzemeHareketFisiService.CreateAsync(
            new MalzemeHareketFisi { Tarih = DateTime.Today.AddDays(-2), HareketTipi = HareketTipi.Cikis, SubeId = subeMerkez.Id },
            [
                new MalzemeHareketFisiKalemi { MalzemeId = m03.Id, Miktar = 50, Aciklama = "Müşteri siparişi sevkiyatı" }
            ]);
        await malzemeHareketFisiService.OnaylaAsync(cikis1.Id);

        var musteriTalebiService = services.GetRequiredService<IMusteriTalebiService>();
        var satisTeklifiService = services.GetRequiredService<ISatisTeklifiService>();
        var satisSiparisiService = services.GetRequiredService<ISatisSiparisiService>();
        var sevkIrsaliyesiService = services.GetRequiredService<ISevkIrsaliyesiService>();
        var satisFaturasiService = services.GetRequiredService<ISatisFaturasiService>();

        // Satış zinciri 1: Akcan Ticaret — talep → teklif → sipariş → irsaliye → fatura (tümü onaylı)
        var talep1 = await musteriTalebiService.CreateAsync(new MusteriTalebi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-25), Icerik = "Boru ve profil malzemeleri için fiyat talebi" });
        var teklif1 = await satisTeklifiService.CreateAsync(
            new SatisTeklifi { CariId = cari1.Id, MusteriTalebiId = talep1.Id, Tarih = DateTime.Today.AddDays(-24), GecerlilikTarihi = DateTime.Today.AddDays(-9) },
            [
                new SatisTeklifiKalemi { MalzemeId = m01.Id, Miktar = 100, BirimFiyat = 62, KdvOrani = 20, Iskonto = 0 },
                new SatisTeklifiKalemi { MalzemeId = m02.Id, Miktar = 80, BirimFiyat = 79, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisTeklifiService.OnaylaAsync(teklif1.Id);

        var siparis1 = await satisSiparisiService.CreateAsync(
            new SatisSiparisi { CariId = cari1.Id, SatisTeklifiId = teklif1.Id, Tarih = DateTime.Today.AddDays(-22) },
            [
                new SatisSiparisiKalemi { MalzemeId = m01.Id, Miktar = 100, BirimFiyat = 62, KdvOrani = 20, Iskonto = 0 },
                new SatisSiparisiKalemi { MalzemeId = m02.Id, Miktar = 80, BirimFiyat = 79, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisSiparisiService.OnaylaAsync(siparis1.Id);

        var irsaliye1 = await sevkIrsaliyesiService.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis1.Id, SubeId = subeMerkez.Id, Tarih = DateTime.Today.AddDays(-20), SevkAdresi = "İstanbul, Kadıköy", AracSofor = "34 ABC 123 - Mehmet Yılmaz" },
            [
                new SevkIrsaliyesiKalemi { MalzemeId = m01.Id, Miktar = 100 },
                new SevkIrsaliyesiKalemi { MalzemeId = m02.Id, Miktar = 80 }
            ]);
        await sevkIrsaliyesiService.OnaylaAsync(irsaliye1.Id);

        var fatura1 = await satisFaturasiService.CreateAsync(
            new SatisFaturasi { CariId = cari1.Id, SevkIrsaliyesiId = irsaliye1.Id, Tarih = DateTime.Today.AddDays(-19), VadeTarihi = DateTime.Today.AddDays(11) },
            [
                new SatisFaturasiKalemi { MalzemeId = m01.Id, Miktar = 100, BirimFiyat = 62, KdvOrani = 20, Iskonto = 0 },
                new SatisFaturasiKalemi { MalzemeId = m02.Id, Miktar = 80, BirimFiyat = 79, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisFaturasiService.OnaylaAsync(fatura1.Id);

        // Satış zinciri 2: Yıldız Otomotiv — sac ve boya alımı
        var talep2 = await musteriTalebiService.CreateAsync(new MusteriTalebi { CariId = cari4.Id, Tarih = DateTime.Today.AddDays(-17), Icerik = "Sac ve boya malzemeleri için fiyat talebi" });
        var teklif2 = await satisTeklifiService.CreateAsync(
            new SatisTeklifi { CariId = cari4.Id, MusteriTalebiId = talep2.Id, Tarih = DateTime.Today.AddDays(-16), GecerlilikTarihi = DateTime.Today.AddDays(-1) },
            [
                new SatisTeklifiKalemi { MalzemeId = m07.Id, Miktar = 40, BirimFiyat = 128, KdvOrani = 20, Iskonto = 0 },
                new SatisTeklifiKalemi { MalzemeId = m11.Id, Miktar = 25, BirimFiyat = 115, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisTeklifiService.OnaylaAsync(teklif2.Id);

        var siparis2 = await satisSiparisiService.CreateAsync(
            new SatisSiparisi { CariId = cari4.Id, SatisTeklifiId = teklif2.Id, Tarih = DateTime.Today.AddDays(-14) },
            [
                new SatisSiparisiKalemi { MalzemeId = m07.Id, Miktar = 40, BirimFiyat = 128, KdvOrani = 20, Iskonto = 0 },
                new SatisSiparisiKalemi { MalzemeId = m11.Id, Miktar = 25, BirimFiyat = 115, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisSiparisiService.OnaylaAsync(siparis2.Id);

        var irsaliye2 = await sevkIrsaliyesiService.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis2.Id, SubeId = subeSakarya.Id, Tarih = DateTime.Today.AddDays(-12), SevkAdresi = "Bursa, Nilüfer", AracSofor = "16 XYZ 789 - Ahmet Demir" },
            [
                new SevkIrsaliyesiKalemi { MalzemeId = m07.Id, Miktar = 40 },
                new SevkIrsaliyesiKalemi { MalzemeId = m11.Id, Miktar = 25 }
            ]);
        await sevkIrsaliyesiService.OnaylaAsync(irsaliye2.Id);

        var fatura2 = await satisFaturasiService.CreateAsync(
            new SatisFaturasi { CariId = cari4.Id, SevkIrsaliyesiId = irsaliye2.Id, Tarih = DateTime.Today.AddDays(-11), VadeTarihi = DateTime.Today.AddDays(19) },
            [
                new SatisFaturasiKalemi { MalzemeId = m07.Id, Miktar = 40, BirimFiyat = 128, KdvOrani = 20, Iskonto = 0 },
                new SatisFaturasiKalemi { MalzemeId = m11.Id, Miktar = 25, BirimFiyat = 115, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisFaturasiService.OnaylaAsync(fatura2.Id);

        // Satış zinciri 3: Anadolu İnşaat — elektrik ve bağlantı elemanı alımı
        var talep3 = await musteriTalebiService.CreateAsync(new MusteriTalebi { CariId = cari7.Id, Tarih = DateTime.Today.AddDays(-10), Icerik = "Elektrik kablosu ve bağlantı elemanı için fiyat talebi" });
        var teklif3 = await satisTeklifiService.CreateAsync(
            new SatisTeklifi { CariId = cari7.Id, MusteriTalebiId = talep3.Id, Tarih = DateTime.Today.AddDays(-9), GecerlilikTarihi = DateTime.Today.AddDays(6) },
            [
                new SatisTeklifiKalemi { MalzemeId = m15.Id, Miktar = 300, BirimFiyat = 17, KdvOrani = 20, Iskonto = 0 },
                new SatisTeklifiKalemi { MalzemeId = m20.Id, Miktar = 800, BirimFiyat = 5, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisTeklifiService.OnaylaAsync(teklif3.Id);

        var siparis3 = await satisSiparisiService.CreateAsync(
            new SatisSiparisi { CariId = cari7.Id, SatisTeklifiId = teklif3.Id, Tarih = DateTime.Today.AddDays(-7) },
            [
                new SatisSiparisiKalemi { MalzemeId = m15.Id, Miktar = 300, BirimFiyat = 17, KdvOrani = 20, Iskonto = 0 },
                new SatisSiparisiKalemi { MalzemeId = m20.Id, Miktar = 800, BirimFiyat = 5, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisSiparisiService.OnaylaAsync(siparis3.Id);

        var irsaliye3 = await sevkIrsaliyesiService.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis3.Id, SubeId = subeMerkez.Id, Tarih = DateTime.Today.AddDays(-5), SevkAdresi = "Ankara, Çankaya", AracSofor = "06 DEF 456 - Hasan Kaya" },
            [
                new SevkIrsaliyesiKalemi { MalzemeId = m15.Id, Miktar = 300 },
                new SevkIrsaliyesiKalemi { MalzemeId = m20.Id, Miktar = 800 }
            ]);
        await sevkIrsaliyesiService.OnaylaAsync(irsaliye3.Id);

        var fatura3 = await satisFaturasiService.CreateAsync(
            new SatisFaturasi { CariId = cari7.Id, SevkIrsaliyesiId = irsaliye3.Id, Tarih = DateTime.Today.AddDays(-4), VadeTarihi = DateTime.Today.AddDays(26) },
            [
                new SatisFaturasiKalemi { MalzemeId = m15.Id, Miktar = 300, BirimFiyat = 17, KdvOrani = 20, Iskonto = 0 },
                new SatisFaturasiKalemi { MalzemeId = m20.Id, Miktar = 800, BirimFiyat = 5, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisFaturasiService.OnaylaAsync(fatura3.Id);

        // Akışın farklı aşamalarında bırakılmış örnek satış belgeleri
        await musteriTalebiService.CreateAsync(new MusteriTalebi { CariId = cari8.Id, Tarih = DateTime.Today.AddDays(-3), Icerik = "Elektrik malzemeleri hakkında teklif talebi" });

        var teklif4 = await satisTeklifiService.CreateAsync(
            new SatisTeklifi { CariId = cari3.Id, Tarih = DateTime.Today.AddDays(-5), GecerlilikTarihi = DateTime.Today.AddDays(10) },
            [
                new SatisTeklifiKalemi { MalzemeId = m03.Id, Miktar = 60, BirimFiyat = 95, KdvOrani = 20, Iskonto = 0 },
                new SatisTeklifiKalemi { MalzemeId = m06.Id, Miktar = 40, BirimFiyat = 260, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisTeklifiService.OnaylaAsync(teklif4.Id);

        var siparis4 = await satisSiparisiService.CreateAsync(
            new SatisSiparisi { CariId = cari1.Id, Tarih = DateTime.Today.AddDays(-4) },
            [
                new SatisSiparisiKalemi { MalzemeId = m08.Id, Miktar = 20, BirimFiyat = 425, KdvOrani = 20, Iskonto = 0 },
                new SatisSiparisiKalemi { MalzemeId = m09.Id, Miktar = 15, BirimFiyat = 245, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisSiparisiService.OnaylaAsync(siparis4.Id);

        var siparis5 = await satisSiparisiService.CreateAsync(
            new SatisSiparisi { CariId = cari4.Id, Tarih = DateTime.Today.AddDays(-6) },
            [
                new SatisSiparisiKalemi { MalzemeId = m16.Id, Miktar = 50, BirimFiyat = 62, KdvOrani = 20, Iskonto = 0 },
                new SatisSiparisiKalemi { MalzemeId = m17.Id, Miktar = 30, BirimFiyat = 48, KdvOrani = 20, Iskonto = 0 }
            ]);
        await satisSiparisiService.OnaylaAsync(siparis5.Id);

        var irsaliye4 = await sevkIrsaliyesiService.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis5.Id, SubeId = subeSakarya.Id, Tarih = DateTime.Today.AddDays(-3), SevkAdresi = "Bursa, Nilüfer", AracSofor = "16 XYZ 789 - Ahmet Demir" },
            [
                new SevkIrsaliyesiKalemi { MalzemeId = m16.Id, Miktar = 50 },
                new SevkIrsaliyesiKalemi { MalzemeId = m17.Id, Miktar = 30 }
            ]);
        await sevkIrsaliyesiService.OnaylaAsync(irsaliye4.Id);

        var alisSiparisiService = services.GetRequiredService<IAlisSiparisiService>();
        var alisIrsaliyesiService = services.GetRequiredService<IAlisIrsaliyesiService>();
        var alisFaturasiService = services.GetRequiredService<IAlisFaturasiService>();

        // Alış zinciri 1: Sakarya Metal — sipariş → irsaliye → fatura (tümü onaylı)
        var alisSiparis1 = await alisSiparisiService.CreateAsync(
            new AlisSiparisi { Tarih = DateTime.Today.AddDays(-18), CariId = cari2.Id, SubeId = subeMerkez.Id, Aciklama = "Boru/profil stok tamamlama siparişi" },
            [
                new AlisSiparisiKalemi { MalzemeId = m01.Id, Miktar = 300, BirimFiyat = 45, KdvOrani = 20, Iskonto = 5 },
                new AlisSiparisiKalemi { MalzemeId = m04.Id, Miktar = 200, BirimFiyat = 68, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisSiparisiService.OnaylaAsync(alisSiparis1.Id);

        var alisIrsaliye1 = await alisIrsaliyesiService.CreateAsync(
            new AlisIrsaliyesi { Tarih = DateTime.Today.AddDays(-16), AlisSiparisiId = alisSiparis1.Id, CariId = cari2.Id, SubeId = subeMerkez.Id },
            [
                new AlisIrsaliyesiKalemi { MalzemeId = m01.Id, Miktar = 300 },
                new AlisIrsaliyesiKalemi { MalzemeId = m04.Id, Miktar = 200 }
            ]);
        await alisIrsaliyesiService.OnaylaAsync(alisIrsaliye1.Id);

        var alisFatura1 = await alisFaturasiService.CreateAsync(
            new AlisFaturasi { Tarih = DateTime.Today.AddDays(-15), CariId = cari2.Id, AlisIrsaliyesiId = alisIrsaliye1.Id },
            [
                new AlisFaturasiKalemi { MalzemeId = m01.Id, Miktar = 300, BirimFiyat = 45, KdvOrani = 20, Iskonto = 5 },
                new AlisFaturasiKalemi { MalzemeId = m04.Id, Miktar = 200, BirimFiyat = 68, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisFaturasiService.OnaylaAsync(alisFatura1.Id);

        // Alış zinciri 2: Ege Kimya — boya/kaplama alımı
        var alisSiparis2 = await alisSiparisiService.CreateAsync(
            new AlisSiparisi { Tarih = DateTime.Today.AddDays(-14), CariId = cari5.Id, SubeId = subeMerkez.Id, Aciklama = "Boya stok tamamlama siparişi" },
            [
                new AlisSiparisiKalemi { MalzemeId = m11.Id, Miktar = 150, BirimFiyat = 85, KdvOrani = 20, Iskonto = 0 },
                new AlisSiparisiKalemi { MalzemeId = m12.Id, Miktar = 80, BirimFiyat = 60, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisSiparisiService.OnaylaAsync(alisSiparis2.Id);

        var alisIrsaliye2 = await alisIrsaliyesiService.CreateAsync(
            new AlisIrsaliyesi { Tarih = DateTime.Today.AddDays(-12), AlisSiparisiId = alisSiparis2.Id, CariId = cari5.Id, SubeId = subeMerkez.Id },
            [
                new AlisIrsaliyesiKalemi { MalzemeId = m11.Id, Miktar = 150 },
                new AlisIrsaliyesiKalemi { MalzemeId = m12.Id, Miktar = 80 }
            ]);
        await alisIrsaliyesiService.OnaylaAsync(alisIrsaliye2.Id);

        var alisFatura2 = await alisFaturasiService.CreateAsync(
            new AlisFaturasi { Tarih = DateTime.Today.AddDays(-11), CariId = cari5.Id, AlisIrsaliyesiId = alisIrsaliye2.Id },
            [
                new AlisFaturasiKalemi { MalzemeId = m11.Id, Miktar = 150, BirimFiyat = 85, KdvOrani = 20, Iskonto = 0 },
                new AlisFaturasiKalemi { MalzemeId = m12.Id, Miktar = 80, BirimFiyat = 60, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisFaturasiService.OnaylaAsync(alisFatura2.Id);

        // Alış zinciri 3: Marmara Çelik — sac alımı
        var alisSiparis3 = await alisSiparisiService.CreateAsync(
            new AlisSiparisi { Tarih = DateTime.Today.AddDays(-10), CariId = cari6.Id, SubeId = subeSakarya.Id, Aciklama = "Sac stok tamamlama siparişi" },
            [
                new AlisSiparisiKalemi { MalzemeId = m06.Id, Miktar = 120, BirimFiyat = 210, KdvOrani = 20, Iskonto = 3 },
                new AlisSiparisiKalemi { MalzemeId = m07.Id, Miktar = 200, BirimFiyat = 95, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisSiparisiService.OnaylaAsync(alisSiparis3.Id);

        var alisIrsaliye3 = await alisIrsaliyesiService.CreateAsync(
            new AlisIrsaliyesi { Tarih = DateTime.Today.AddDays(-8), AlisSiparisiId = alisSiparis3.Id, CariId = cari6.Id, SubeId = subeSakarya.Id },
            [
                new AlisIrsaliyesiKalemi { MalzemeId = m06.Id, Miktar = 120 },
                new AlisIrsaliyesiKalemi { MalzemeId = m07.Id, Miktar = 200 }
            ]);
        await alisIrsaliyesiService.OnaylaAsync(alisIrsaliye3.Id);

        var alisFatura3 = await alisFaturasiService.CreateAsync(
            new AlisFaturasi { Tarih = DateTime.Today.AddDays(-7), CariId = cari6.Id, AlisIrsaliyesiId = alisIrsaliye3.Id },
            [
                new AlisFaturasiKalemi { MalzemeId = m06.Id, Miktar = 120, BirimFiyat = 210, KdvOrani = 20, Iskonto = 3 },
                new AlisFaturasiKalemi { MalzemeId = m07.Id, Miktar = 200, BirimFiyat = 95, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisFaturasiService.OnaylaAsync(alisFatura3.Id);

        // Akışın farklı aşamalarında bırakılmış örnek alış belgeleri
        var alisSiparis4 = await alisSiparisiService.CreateAsync(
            new AlisSiparisi { Tarih = DateTime.Today.AddDays(-2), CariId = cari2.Id, SubeId = subeMerkez.Id, Aciklama = "Ek boru siparişi" },
            [
                new AlisSiparisiKalemi { MalzemeId = m05.Id, Miktar = 100, BirimFiyat = 81, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisSiparisiService.OnaylaAsync(alisSiparis4.Id);

        var alisSiparis5 = await alisSiparisiService.CreateAsync(
            new AlisSiparisi { Tarih = DateTime.Today.AddDays(-5), CariId = cari8.Id, SubeId = subeMerkez.Id, Aciklama = "Elektrik malzemeleri siparişi" },
            [
                new AlisSiparisiKalemi { MalzemeId = m18.Id, Miktar = 40, BirimFiyat = 180, KdvOrani = 20, Iskonto = 0 }
            ]);
        await alisSiparisiService.OnaylaAsync(alisSiparis5.Id);

        await alisIrsaliyesiService.CreateAsync(
            new AlisIrsaliyesi { Tarih = DateTime.Today.AddDays(-3), AlisSiparisiId = alisSiparis5.Id, CariId = cari8.Id, SubeId = subeMerkez.Id },
            [
                new AlisIrsaliyesiKalemi { MalzemeId = m18.Id, Miktar = 40 }
            ]);
    }
}
