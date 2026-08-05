using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class AlisFaturasiServiceTests
{
    private static async Task<(AppDbContext Baglam, AlisFaturasiService Servis, Cari Cari, Malzeme Malzeme)> SenaryoKur(decimal baslangicBakiye = 100)
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur(baslangicBakiye);
        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new AlisFaturasiService(unitOfWork, onayYetkisiService), cari, malzeme);
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

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Tedarikçi", CariTipi = CariTipi.Tedarikci };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = baslangicBakiye };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);

        foreach (var (kod, ad) in new[] { ("153", "Ticari Mallar"), ("191", "İndirilecek KDV"), ("320", "Satıcılar") })
            baglam.HesapPlani.Add(new HesapPlani { HesapKodu = kod, HesapAdi = ad, HesapTipi = HesapTipi.Aktif });

        await baglam.SaveChangesAsync();
        return (baglam, unitOfWork, cari, malzeme);
    }

    private static AlisFaturasiService ServisOlustur(UnitOfWork unitOfWork, decimal? yuksekTutarEsigi, string? kullaniciRolu)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(yuksekTutarEsigi is null
                ? []
                : new Dictionary<string, string?> { ["OnayAyarlari:YuksekTutarEsigi"] = yuksekTutarEsigi.Value.ToString() })
            .Build();
        var kimlik = kullaniciRolu is null ? new ClaimsIdentity() : new ClaimsIdentity([new Claim(ClaimTypes.Role, kullaniciRolu)], "TestAuth");
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(kimlik) } };
        var onayYetkisiService = new OnayYetkisiService(httpContextAccessor, config);
        return new AlisFaturasiService(unitOfWork, onayYetkisiService);
    }

    private static AlisFaturasi YeniFatura(int cariId) => new()
    {
        CariId = cariId,
        Tarih = DateTime.Today,
        Kalemler = []
    };

    private static List<AlisFaturasiKalemi> Kalemler(int malzemeId, decimal miktar = 10) =>
        [new AlisFaturasiKalemi { MalzemeId = malzemeId, Miktar = miktar, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }];

    [Fact]
    public async Task OnaylaAsync_IrsaliyesizFatura_StokArttirirCariAzaltirMuhasebeFisiOlusturur()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));

        await servis.OnaylaAsync(fatura.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(110, guncelMalzeme!.Bakiye);
        Assert.Equal(-600, guncelCari!.Bakiye);
        Assert.Single(baglam.CariFisleri.Where(f => f.AlisFaturasiId == fatura.Id));
        Assert.Single(baglam.MuhasebeFisleri.Where(m => m.AlisFaturasiId == fatura.Id));
    }

    [Fact]
    public async Task OnaylaAsync_SubeliFatura_OtomatikOlusanCariFisiAyniSubeyiAlir()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var sube = new Sube { SubeAdi = "Test Şube" };
        baglam.Subeler.Add(sube);
        await baglam.SaveChangesAsync();

        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        fatura.SubeId = sube.Id;
        await baglam.SaveChangesAsync();

        await servis.OnaylaAsync(fatura.Id);

        var cariFisi = await baglam.CariFisleri.SingleAsync(f => f.AlisFaturasiId == fatura.Id);
        Assert.Equal(sube.Id, cariFisi.SubeId);
    }

    [Fact]
    public async Task OnaylaAsync_IskontoluKalem_MalzemeAlisFiyatiIskontoDusulmusFiyatiAlir()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id),
            [new AlisFaturasiKalemi { MalzemeId = malzeme.Id, Miktar = 10, BirimFiyat = 50, KdvOrani = 20, Iskonto = 10 }]);

        await servis.OnaylaAsync(fatura.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(45, guncelMalzeme!.AlisFiyati);
    }

    [Fact]
    public async Task OnaylaAsync_Onaylandiginda_MalzemeAlisFiyatiKalemBirimFiyatinaGuncellenir()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        malzeme.AlisFiyati = 10;
        await baglam.SaveChangesAsync();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id)); // BirimFiyat: 50

        await servis.OnaylaAsync(fatura.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(50, guncelMalzeme!.AlisFiyati);
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
        var guncelFatura = await baglam.AlisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(100, guncelMalzeme!.Bakiye);
        Assert.Equal(0, guncelCari!.Bakiye);
        Assert.Equal(BelgeDurum.Iptal, guncelFatura!.Durum);
        Assert.True(baglam.CariFisleri.IgnoreQueryFilters().Single(f => f.AlisFaturasiId == fatura.Id).IsDeleted);
        Assert.True(baglam.MuhasebeFisleri.IgnoreQueryFilters().Single(m => m.AlisFaturasiId == fatura.Id).IsDeleted);
    }

    [Fact]
    public async Task IptalEtAsync_GeriAlinacakMiktarStoguNegatifeDusururse_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, servis, cari, malzeme) = await SenaryoKur();
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id, miktar: 10));
        await servis.OnaylaAsync(fatura.Id);

        // Onaydan sonra malzemenin 105 birimi başka bir işlemle (örn. satış) düşülmüş olsun.
        malzeme.Bakiye = 5;
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.IptalEtAsync(fatura.Id));
        Assert.Contains("negatife düşürür", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelFatura = await baglam.AlisFaturalari.FindAsync(fatura.Id);
        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelFatura!.Durum);
        Assert.Equal(-600, guncelCari!.Bakiye);
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

        var muhasebeServis = ServisOlustur(unitOfWork, yuksekTutarEsigi: 500, kullaniciRolu: "Muhasebe");
        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => muhasebeServis.IptalEtAsync(fatura.Id));
        Assert.Contains("sadece Admin", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelFatura = await baglam.AlisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelFatura!.Durum);
    }

    private static HttpContextAccessor KullaniciBaglamiOlustur(string kullaniciAdi, string rol) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, kullaniciAdi), new Claim(ClaimTypes.Role, rol)], "TestAuth")) } };

    [Fact]
    public async Task OnaylaAsync_OlusturanKendiFaturasiniOnaylamayaCalisir_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var servis = new AlisFaturasiService(unitOfWork, new OnayYetkisiService(new HttpContextAccessor(), config));
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        fatura.CreatedBy = "satinalma@test.com";
        await baglam.SaveChangesAsync();

        var olusturanServis = new AlisFaturasiService(unitOfWork, new OnayYetkisiService(KullaniciBaglamiOlustur("satinalma@test.com", "Muhasebe"), config));
        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => olusturanServis.OnaylaAsync(fatura.Id));
        Assert.Contains("kendi belgesini onaylayamaz", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelFatura = await baglam.AlisFaturalari.FindAsync(fatura.Id);
        Assert.Equal(BelgeDurum.Beklemede, guncelFatura!.Durum);
    }

    [Fact]
    public async Task OnaylaAsync_BaskaKullaniciOnaylar_BasariylaOnaylanir()
    {
        var (baglam, unitOfWork, cari, malzeme) = await TemelVeriKur();
        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var servis = new AlisFaturasiService(unitOfWork, new OnayYetkisiService(new HttpContextAccessor(), config));
        var fatura = await servis.CreateAsync(YeniFatura(cari.Id), Kalemler(malzeme.Id));
        fatura.CreatedBy = "satinalma@test.com";
        await baglam.SaveChangesAsync();

        var onaylayanServis = new AlisFaturasiService(unitOfWork, new OnayYetkisiService(KullaniciBaglamiOlustur("muhasebe@test.com", "Muhasebe"), config));
        await onaylayanServis.OnaylaAsync(fatura.Id);

        var guncelFatura = await baglam.AlisFaturalari.FindAsync(fatura.Id);
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

        // Hata fırlatılmadıysa test zaten geçer; ek doğrulama gerekmiyor.
    }
}
