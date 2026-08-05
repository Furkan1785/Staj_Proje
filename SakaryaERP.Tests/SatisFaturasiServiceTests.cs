using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class SatisFaturasiServiceTests
{
    private static async Task<(AppDbContext Baglam, SatisFaturasiService Servis, Cari Cari, Malzeme Malzeme)> SenaryoKur(decimal baslangicBakiye = 100)
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur(baslangicBakiye);
        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new SatisFaturasiService(unitOfWork, onayYetkisiService), cari, malzeme);
    }

    // Rol/eşik senaryolarında servis, HttpContextAccessor'ı ayarlayan koddan hemen sonra AYNI
    // metotta (await sınırı geçmeden) inşa edilmeli — aksi halde IHttpContextAccessor'ın
    // AsyncLocal tabanlı durumu, içinde await barındıran bu paylaşılan kurulum metodundan
    // döndükten sonra çağırana taşınmaz (ExecutionContext sadece iç içe çağrılara akar, geri
    // dönüşe değil). Bu yüzden burada sadece ham malzemeleri (unitOfWork dahil) döndürüyoruz.
    private static async Task<(AppDbContext Baglam, UnitOfWork UnitOfWork, Cari Cari, Malzeme Malzeme)> TemelVeriKur(decimal baslangicBakiye = 100)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = baslangicBakiye };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);

        foreach (var (kod, ad) in new[] { ("120", "Alıcılar"), ("600", "Yurtiçi Satışlar"), ("391", "Hesaplanan KDV") })
            baglam.HesapPlani.Add(new HesapPlani { HesapKodu = kod, HesapAdi = ad, HesapTipi = HesapTipi.Aktif });

        await baglam.SaveChangesAsync();
        return (baglam, unitOfWork, cari, malzeme);
    }

    private static SatisFaturasiService ServisOlustur(UnitOfWork unitOfWork, decimal? yuksekTutarEsigi, string? kullaniciRolu)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(yuksekTutarEsigi is null
                ? []
                : new Dictionary<string, string?> { ["OnayAyarlari:YuksekTutarEsigi"] = yuksekTutarEsigi.Value.ToString() })
            .Build();
        var kimlik = kullaniciRolu is null ? new ClaimsIdentity() : new ClaimsIdentity([new Claim(ClaimTypes.Role, kullaniciRolu)], "TestAuth");
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(kimlik) } };
        var onayYetkisiService = new OnayYetkisiService(httpContextAccessor, config);
        return new SatisFaturasiService(unitOfWork, onayYetkisiService);
    }

    private static SatisFaturasi YeniFatura(int cariId) => new()
    {
        CariId = cariId,
        Tarih = DateTime.Today,
        Kalemler = []
    };

    private static List<SatisFaturasiKalemi> Kalemler(int malzemeId, decimal miktar = 10) =>
        [new SatisFaturasiKalemi { MalzemeId = malzemeId, Miktar = miktar, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }];

    [Fact]
    public async Task OnaylaAsync_IrsaliyesizFatura_StokDusururCariArttirirMuhasebeFisiOlusturur()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));

        await servis.OnaylaAsync(fatura.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(90, guncelMalzeme!.Bakiye);
        Assert.Equal(600, guncelCari!.Bakiye);
        Assert.Single(baglam.CariFisleri.Where(f => f.SatisFaturasiId == fatura.Id));
        Assert.Single(baglam.MuhasebeFisleri.Where(m => m.SatisFaturasiId == fatura.Id));
    }

    [Fact]
    public async Task OnaylaAsync_MaliyetliMalzeme_BirimMaliyetSnapshotAlirVeCogsYevmiyeKaydiOlusturur()
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        malzeme.AlisFiyati = 30;
        baglam.HesapPlani.Add(new HesapPlani { HesapKodu = "621", HesapAdi = "Satılan Ticari Mallar Maliyeti", HesapTipi = HesapTipi.Gider });
        baglam.HesapPlani.Add(new HesapPlani { HesapKodu = "153", HesapAdi = "Ticari Mallar", HesapTipi = HesapTipi.Aktif });
        await baglam.SaveChangesAsync();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        var servis = new SatisFaturasiService(unitOfWork, onayYetkisiService);
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id, miktar: 10));

        await servis.OnaylaAsync(fatura.Id);

        var guncelKalem = await baglam.SatisFaturasiKalemleri.FirstAsync(k => k.SatisFaturasiId == fatura.Id);
        Assert.Equal(30, guncelKalem.BirimMaliyet);

        var muhasebeFisi = await baglam.MuhasebeFisleri
            .Include(m => m.Kalemler).ThenInclude(k => k.HesapPlani)
            .FirstAsync(m => m.SatisFaturasiId == fatura.Id);
        var cogsBorc = muhasebeFisi.Kalemler.Single(k => k.HesapPlani.HesapKodu == "621");
        var cogsAlacak = muhasebeFisi.Kalemler.Single(k => k.HesapPlani.HesapKodu == "153");
        Assert.Equal(300, cogsBorc.Borc);
        Assert.Equal(300, cogsAlacak.Alacak);
    }

    [Fact]
    public async Task IptalEtAsync_OnaylanmisIrsaliyesizFatura_StokCariMuhasebeTersineCevirir()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        await servis.OnaylaAsync(fatura.Id);

        await servis.IptalEtAsync(fatura.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelFatura = await baglam.SatisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(100, guncelMalzeme!.Bakiye);
        Assert.Equal(0, guncelCari!.Bakiye);
        Assert.Equal(BelgeDurum.Iptal, guncelFatura!.Durum);
        Assert.True(baglam.CariFisleri.IgnoreQueryFilters().Single(f => f.SatisFaturasiId == fatura.Id).IsDeleted);
        Assert.True(baglam.MuhasebeFisleri.IgnoreQueryFilters().Single(m => m.SatisFaturasiId == fatura.Id).IsDeleted);
    }

    [Fact]
    public async Task IptalEtAsync_ZatenIptalEdilmisFatura_HataFirlatir()
    {
        var (_, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        await servis.IptalEtAsync(fatura.Id);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.IptalEtAsync(fatura.Id));
        Assert.Contains("zaten iptal edilmiş", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IptalEtAsync_OnaylıYuksekTutarliFaturaAdminOlmayanKullanici_HataFirlatirVeBirSeyDegismez()
    {
        // Fatura toplamı: 10 * 50 * 1.20 = 600 — onay Admin ile yapılır (Kademe 2 kuralı onayı da
        // etkiler), iptal denemesi Admin olmayan bir kullanıcıyla yapılır.
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var adminServis = ServisOlustur(unitOfWork, yuksekTutarEsigi: 500, kullaniciRolu: "Admin");
        var fatura = await adminServis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        await adminServis.OnaylaAsync(fatura.Id);

        var satisServis = ServisOlustur(unitOfWork, yuksekTutarEsigi: 500, kullaniciRolu: "Satis");
        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => satisServis.IptalEtAsync(fatura.Id));
        Assert.Contains("sadece Admin", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelFatura = await baglam.SatisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelFatura!.Durum);
    }

    private static HttpContextAccessor KullaniciBaglamiOlustur(string kullaniciAdi, string rol) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, kullaniciAdi), new Claim(ClaimTypes.Role, rol)], "TestAuth")) } };

    [Fact]
    public async Task OnaylaAsync_OlusturanKendiFaturasiniOnaylamayaCalisir_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var servis = new SatisFaturasiService(unitOfWork, new OnayYetkisiService(new HttpContextAccessor(), config));
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        fatura.CreatedBy = "satis@test.com";
        await baglam.SaveChangesAsync();

        var olusturanServis = new SatisFaturasiService(unitOfWork, new OnayYetkisiService(KullaniciBaglamiOlustur("satis@test.com", "Satis"), config));
        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => olusturanServis.OnaylaAsync(fatura.Id));
        Assert.Contains("kendi belgesini onaylayamaz", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelFatura = await baglam.SatisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(BelgeDurum.Beklemede, guncelFatura!.Durum);
    }

    [Fact]
    public async Task OnaylaAsync_BaskaKullaniciOnaylar_BasariylaOnaylanir()
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var servis = new SatisFaturasiService(unitOfWork, new OnayYetkisiService(new HttpContextAccessor(), config));
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        fatura.CreatedBy = "satis@test.com";
        await baglam.SaveChangesAsync();

        var onaylayanServis = new SatisFaturasiService(unitOfWork, new OnayYetkisiService(KullaniciBaglamiOlustur("muhasebe@test.com", "Muhasebe"), config));
        await onaylayanServis.OnaylaAsync(fatura.Id);

        var guncelFatura = await baglam.SatisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelFatura!.Durum);
    }

    [Fact]
    public async Task IptalEtAsync_OnaylıYuksekTutarliFaturaAdminKullanici_BasariylaIptalOlur()
    {
        var (_, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var servis = ServisOlustur(unitOfWork, yuksekTutarEsigi: 500, kullaniciRolu: "Admin");
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        await servis.OnaylaAsync(fatura.Id);

        await servis.IptalEtAsync(fatura.Id);
    }
}
