using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class MalzemeHareketFisiServiceTests
{
    private static async Task<(AppDbContext Baglam, MalzemeHareketFisiService Servis, Malzeme Malzeme, MalzemeHareketFisi Fis)>
        SenaryoKur(HareketTipi hareketTipi, decimal baslangicBakiye, decimal fisMiktari)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var sube = new Sube { SubeAdi = "Merkez Şube" };
        var malzeme = new Malzeme
        {
            MalzemeKodu = "M001",
            MalzemeAdi = "Test Malzemesi",
            Birim = "Adet",
            Bakiye = baslangicBakiye
        };
        baglam.Subeler.Add(sube);
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        var fis = new MalzemeHareketFisi
        {
            FisNo = "MH-000001",
            Tarih = DateTime.Today,
            HareketTipi = hareketTipi,
            SubeId = sube.Id,
            Durum = BelgeDurum.Beklemede,
            Kalemler = [new MalzemeHareketFisiKalemi { MalzemeId = malzeme.Id, Miktar = fisMiktari }]
        };
        baglam.MalzemeHareketFisleri.Add(fis);
        await baglam.SaveChangesAsync();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new MalzemeHareketFisiService(unitOfWork, onayYetkisiService), malzeme, fis);
    }

    [Fact]
    public async Task OnaylaAsync_CikisMiktariMevcutBakiyeyiAsarsa_HataFirlatir()
    {
        var (_, servis, _, fis) = await SenaryoKur(HareketTipi.Cikis, baslangicBakiye: 10, fisMiktari: 15);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.OnaylaAsync(fis.Id));

        Assert.Contains("stok yetersiz", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnaylaAsync_CikisMiktariYeterliyse_BakiyeyiDusurur()
    {
        var (baglam, servis, malzeme, fis) = await SenaryoKur(HareketTipi.Cikis, baslangicBakiye: 10, fisMiktari: 4);

        await servis.OnaylaAsync(fis.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(6, guncelMalzeme!.Bakiye);
    }

    [Fact]
    public async Task OnaylaAsync_GirisFisi_BakiyeyiArtirir()
    {
        var (baglam, servis, malzeme, fis) = await SenaryoKur(HareketTipi.Giris, baslangicBakiye: 10, fisMiktari: 4);

        await servis.OnaylaAsync(fis.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(14, guncelMalzeme!.Bakiye);
    }

    [Fact]
    public async Task OnaylaAsync_ZatenOnaylanmisFis_HataFirlatir()
    {
        var (_, servis, _, fis) = await SenaryoKur(HareketTipi.Giris, baslangicBakiye: 10, fisMiktari: 4);
        await servis.OnaylaAsync(fis.Id);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.OnaylaAsync(fis.Id));

        Assert.Contains("beklemedeki", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IptalEtAsync_BeklemedeFis_DurumIptalOlurBakiyeDegismez()
    {
        var (baglam, servis, malzeme, fis) = await SenaryoKur(HareketTipi.Giris, baslangicBakiye: 10, fisMiktari: 4);

        await servis.IptalEtAsync(fis.Id);

        var guncelFis = await baglam.MalzemeHareketFisleri.FindAsync(fis.Id);
        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(BelgeDurum.Iptal, guncelFis!.Durum);
        Assert.Equal(10, guncelMalzeme!.Bakiye);
    }

    [Fact]
    public async Task IptalEtAsync_ZatenOnaylanmisFis_HataFirlatir()
    {
        var (_, servis, _, fis) = await SenaryoKur(HareketTipi.Giris, baslangicBakiye: 10, fisMiktari: 4);
        await servis.OnaylaAsync(fis.Id);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.IptalEtAsync(fis.Id));

        Assert.Contains("beklemede", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSayfaliListeAsync_SubeliKullanici_SadeceKendiSubesininFislerini_ListelerVeExcelExportEsasAlir()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var subeA = new Sube { SubeAdi = "A Şubesi" };
        var subeB = new Sube { SubeAdi = "B Şubesi" };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet" };
        baglam.Subeler.AddRange(subeA, subeB);
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        baglam.MalzemeHareketFisleri.AddRange(
            new MalzemeHareketFisi
            {
                FisNo = "MH-000001", Tarih = DateTime.Today, HareketTipi = HareketTipi.Giris, SubeId = subeA.Id,
                Kalemler = [new MalzemeHareketFisiKalemi { MalzemeId = malzeme.Id, Miktar = 1 }]
            },
            new MalzemeHareketFisi
            {
                FisNo = "MH-000002", Tarih = DateTime.Today, HareketTipi = HareketTipi.Giris, SubeId = subeB.Id,
                Kalemler = [new MalzemeHareketFisiKalemi { MalzemeId = malzeme.Id, Miktar = 1 }]
            });
        await baglam.SaveChangesAsync();

        var httpContext = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                    [
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Satis"),
                        new System.Security.Claims.Claim("SubeId", subeA.Id.ToString())
                    ], "TestAuth"))
            }
        };
        var onayYetkisiService = new OnayYetkisiService(httpContext, new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        var servis = new MalzemeHareketFisiService(unitOfWork, onayYetkisiService);

        // Excel export da aynı metodu (int.MaxValue sayfa boyutuyla) kullanıyor, bu yüzden
        // tek bir test hem DataTables listesini hem export'u kapsıyor.
        var (kayitlar, toplamKayit, filtrelenmisKayit) = await servis.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[6], -1, "desc");

        var kayitListesi = kayitlar.ToList();
        Assert.Single(kayitListesi);
        Assert.Equal("MH-000001", kayitListesi[0].FisNo);
        Assert.Equal(1, toplamKayit);
        Assert.Equal(1, filtrelenmisKayit);
    }
}
