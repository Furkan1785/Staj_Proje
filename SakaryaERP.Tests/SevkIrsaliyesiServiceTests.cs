using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class SevkIrsaliyesiServiceTests
{
    private static async Task<(AppDbContext Baglam, SevkIrsaliyesiService Servis, Sube Sube, Malzeme Malzeme, SatisSiparisi Siparis)>
        SenaryoKur(decimal siparisMiktari)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var sube = new Sube { SubeAdi = "Merkez Şube" };
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = 100 };
        baglam.Subeler.Add(sube);
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        var siparis = new SatisSiparisi
        {
            SiparisNo = "SS-000001",
            CariId = cari.Id,
            Tarih = DateTime.Today,
            Durum = BelgeDurum.Onaylandi,
            Kalemler = [new SatisSiparisiKalemi { MalzemeId = malzeme.Id, Miktar = siparisMiktari, BirimFiyat = 10, KdvOrani = 20, Iskonto = 0 }]
        };
        baglam.SatisSiparisleri.Add(siparis);
        await baglam.SaveChangesAsync();

        return (baglam, new SevkIrsaliyesiService(unitOfWork), sube, malzeme, siparis);
    }

    [Fact]
    public async Task CreateAsync_SiparisMiktariniAsanSevkiyat_HataFirlatir()
    {
        var (baglam, servis, sube, malzeme, siparis) = await SenaryoKur(siparisMiktari: 10);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 15 }]));

        Assert.Contains("sipariş miktarını aşıyor", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_KismiSevkiyatSonrasiKalanMiktariAsanIkinciSevkiyat_HataFirlatir()
    {
        var (baglam, servis, sube, malzeme, siparis) = await SenaryoKur(siparisMiktari: 10);

        // İlk sevkiyat: 6 birim (kalan 4)
        await servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 6 }]);

        // İkinci sevkiyat: 5 birim iste (kalan sadece 4) — reddedilmeli
        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 5 }]));

        Assert.Contains("kalan: 4", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_KalanMiktarKadarSevkiyat_BasariylaOlusur()
    {
        var (baglam, servis, sube, malzeme, siparis) = await SenaryoKur(siparisMiktari: 10);

        await servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 6 }]);

        var irsaliye = await servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 4 }]);

        Assert.Equal("SI-000002", irsaliye.IrsaliyeNo);
    }

    [Fact]
    public async Task OnaylaAsync_StokYetersizse_HataFirlatirVeBakiyeDegismez()
    {
        var (baglam, servis, sube, malzeme, siparis) = await SenaryoKur(siparisMiktari: 200);
        malzeme.Bakiye = 3;
        await baglam.SaveChangesAsync();

        var irsaliye = await servis.CreateAsync(
            new SevkIrsaliyesi { SatisSiparisiId = siparis.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new SevkIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 5 }]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.OnaylaAsync(irsaliye.Id));

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(3, guncelMalzeme!.Bakiye);
    }
}
