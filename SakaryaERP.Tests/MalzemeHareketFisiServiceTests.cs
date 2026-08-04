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

        return (baglam, new MalzemeHareketFisiService(unitOfWork), malzeme, fis);
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
}
