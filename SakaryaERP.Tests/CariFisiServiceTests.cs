using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class CariFisiServiceTests
{
    private static (AppDbContext Baglam, CariFisiService Servis, Cari Cari, BankaHesabi Banka, KasaHesabi Kasa) SenaryoKur()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = CariTipi.HerIkisi };
        var banka = new BankaHesabi { HesapAdi = "Test Banka", BankaAdi = "X Bank", Bakiye = 1000 };
        var kasa = new KasaHesabi { KasaAdi = "Merkez Kasa", Bakiye = 1000 };
        baglam.Cariler.Add(cari);
        baglam.BankaHesaplari.Add(banka);
        baglam.KasaHesaplari.Add(kasa);
        baglam.SaveChangesAsync().Wait();

        return (baglam, new CariFisiService(unitOfWork), cari, banka, kasa);
    }

    [Fact]
    public async Task CreateAsync_BorcFisiNakit_CariBakiyesiniArtirirKasayiAzaltir()
    {
        var (baglam, servis, cari, _, kasa) = SenaryoKur();

        await servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Borc,
            Tutar = 500,
            OdemeYontemi = OdemeYontemi.Nakit,
            KasaHesabiId = kasa.Id
        });

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelKasa = await baglam.KasaHesaplari.FindAsync(kasa.Id);
        Assert.Equal(500, guncelCari!.Bakiye);
        Assert.Equal(500, guncelKasa!.Bakiye);
    }

    [Fact]
    public async Task CreateAsync_AlacakFisiHavale_CariBakiyesiniAzaltirBankayiArtirir()
    {
        var (baglam, servis, cari, banka, _) = SenaryoKur();

        await servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Alacak,
            Tutar = 300,
            OdemeYontemi = OdemeYontemi.Havale,
            BankaHesabiId = banka.Id
        });

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelBanka = await baglam.BankaHesaplari.FindAsync(banka.Id);
        Assert.Equal(-300, guncelCari!.Bakiye);
        Assert.Equal(1300, guncelBanka!.Bakiye);
    }

    [Fact]
    public async Task CreateAsync_SifirVeyaNegatifTutar_HataFirlatir()
    {
        var (_, servis, cari, _, kasa) = SenaryoKur();

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Borc,
            Tutar = 0,
            OdemeYontemi = OdemeYontemi.Nakit,
            KasaHesabiId = kasa.Id
        }));
    }

    [Fact]
    public async Task CreateAsync_NakitOdemeKasaSecilmeden_HataFirlatir()
    {
        var (_, servis, cari, _, _) = SenaryoKur();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Borc,
            Tutar = 100,
            OdemeYontemi = OdemeYontemi.Nakit
        }));

        Assert.Contains("kasa hesabı", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IptalEtAsync_ManuelFis_BakiyeyiTersYondeGeriAlir()
    {
        var (baglam, servis, cari, _, kasa) = SenaryoKur();

        var fis = await servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Borc,
            Tutar = 500,
            OdemeYontemi = OdemeYontemi.Nakit,
            KasaHesabiId = kasa.Id
        });

        await servis.IptalEtAsync(fis.Id);

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelKasa = await baglam.KasaHesaplari.FindAsync(kasa.Id);
        Assert.Equal(0, guncelCari!.Bakiye);
        Assert.Equal(1000, guncelKasa!.Bakiye);
    }

    [Fact]
    public async Task IptalEtAsync_OtomatikOlusturulmusFis_HataFirlatirVeBakiyeDegismez()
    {
        var (baglam, servis, cari, _, kasa) = SenaryoKur();

        var fis = await servis.CreateAsync(new CariFisi
        {
            CariId = cari.Id,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Borc,
            Tutar = 500,
            OdemeYontemi = OdemeYontemi.Nakit,
            KasaHesabiId = kasa.Id,
            OtomatikOlusturuldu = true
        });

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.IptalEtAsync(fis.Id));
        Assert.Contains("otomatik oluşturulmuştur", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        Assert.Equal(500, guncelCari!.Bakiye);
    }
}
