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
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Tedarikçi", CariTipi = CariTipi.Tedarikci };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = baslangicBakiye };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);

        foreach (var (kod, ad) in new[] { ("153", "Ticari Mallar"), ("191", "İndirilecek KDV"), ("320", "Satıcılar") })
            baglam.HesapPlani.Add(new HesapPlani { HesapKodu = kod, HesapAdi = ad, HesapTipi = HesapTipi.Aktif });

        await baglam.SaveChangesAsync();

        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), config);

        return (baglam, new AlisFaturasiService(unitOfWork, onayYetkisiService), cari, malzeme);
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
}
