using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class CariServiceTests
{
    private static (AppDbContext Baglam, CariService Servis, Cari Cari) SenaryoKur(CariTipi cariTipi)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = cariTipi };
        baglam.Cariler.Add(cari);
        baglam.SaveChangesAsync().Wait();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new CariService(new CariRepository(baglam), unitOfWork, onayYetkisiService), cari);
    }

    [Fact]
    public async Task UpdateAsync_SatisSiparisiOlanCariMusteridenTedarikciyeDaraltilamaz_HataFirlatir()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Musteri);
        baglam.SatisSiparisleri.Add(new SatisSiparisi { SiparisNo = "SS-000001", CariId = cari.Id, Tarih = DateTime.Today });
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.UpdateAsync(new Cari
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = CariTipi.Tedarikci
        }));

        Assert.Contains("daraltılamaz", hata.Message, StringComparison.OrdinalIgnoreCase);
        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(CariTipi.Musteri, guncelCari!.CariTipi);
    }

    [Fact]
    public async Task UpdateAsync_AlisFaturasiOlanCariTedarikciddenMusteriyeDaraltilamaz_HataFirlatir()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Tedarikci);
        baglam.AlisFaturalari.Add(new AlisFaturasi { FaturaNo = "AF-000001", CariId = cari.Id, Tarih = DateTime.Today });
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.UpdateAsync(new Cari
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = CariTipi.Musteri
        }));

        Assert.Contains("daraltılamaz", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_BelgesiOlmayanCarininTipiSerbestceDegisebilir()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Musteri);

        await servis.UpdateAsync(new Cari
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = CariTipi.Tedarikci
        });

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(CariTipi.Tedarikci, guncelCari!.CariTipi);
    }

    [Fact]
    public async Task UpdateAsync_MusteridenHerIkisineGenisletmeHerZamanSerbest()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Musteri);
        baglam.SatisSiparisleri.Add(new SatisSiparisi { SiparisNo = "SS-000001", CariId = cari.Id, Tarih = DateTime.Today });
        await baglam.SaveChangesAsync();

        await servis.UpdateAsync(new Cari
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = CariTipi.HerIkisi
        });

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(CariTipi.HerIkisi, guncelCari!.CariTipi);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirOlmayanCari_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Musteri);
        cari.Bakiye = 500;
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.PasifYapAsync(cari.Id));
        Assert.Contains("kapanmamış bir bakiyesi", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.False(guncelCari!.IsDeleted);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirCari_BasariylaPasifYapilir()
    {
        var (baglam, servis, cari) = SenaryoKur(CariTipi.Musteri);

        await servis.PasifYapAsync(cari.Id);

        var guncelCari = await baglam.Cariler.IgnoreQueryFilters().FirstAsync(c => c.Id == cari.Id);
        Assert.True(guncelCari.IsDeleted);
    }

    [Fact]
    public async Task GetSayfaliListeAsync_SubeliKullanici_KendiSubesiniVeSubesizEskiKayitlariGorurBaskaSubeyiGormez()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var subeA = new Cari { CariKodu = "C001", Unvan = "A Şubesi Carisi", CariTipi = CariTipi.Musteri, SubeId = 1 };
        var subeB = new Cari { CariKodu = "C002", Unvan = "B Şubesi Carisi", CariTipi = CariTipi.Musteri, SubeId = 2 };
        var eskiKayit = new Cari { CariKodu = "C003", Unvan = "Eski Kayıt", CariTipi = CariTipi.Musteri, SubeId = null };
        baglam.Cariler.AddRange(subeA, subeB, eskiKayit);
        await baglam.SaveChangesAsync();

        var httpContext = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                    [
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Satis"),
                        new System.Security.Claims.Claim("SubeId", "1")
                    ], "TestAuth"))
            }
        };
        var onayYetkisiService = new OnayYetkisiService(httpContext, new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        var servis = new CariService(new CariRepository(baglam), unitOfWork, onayYetkisiService);

        // Excel export da aynı metodu (int.MaxValue sayfa boyutuyla) kullanıyor, bu yüzden
        // tek bir test hem DataTables listesini hem export'u kapsıyor.
        var (kayitlar, toplamKayit, filtrelenmisKayit) = await servis.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[7], -1, "desc");

        var kodlar = kayitlar.Select(c => c.CariKodu).ToList();
        Assert.Contains("C001", kodlar);
        Assert.Contains("C003", kodlar);
        Assert.DoesNotContain("C002", kodlar);
        Assert.Equal(2, toplamKayit);
        Assert.Equal(2, filtrelenmisKayit);
    }
}
