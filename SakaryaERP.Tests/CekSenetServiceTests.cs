using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class CekSenetServiceTests
{
    private static async Task<(AppDbContext Baglam, CekSenetService Servis, Cari Cari, KasaHesabi Kasa)> SenaryoKur()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        var cariFisiService = new CariFisiService(unitOfWork, onayYetkisiService);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var kasa = new KasaHesabi { KasaAdi = "Merkez Kasa", Bakiye = 1000 };
        baglam.Cariler.Add(cari);
        baglam.KasaHesaplari.Add(kasa);
        await baglam.SaveChangesAsync();

        return (baglam, new CekSenetService(unitOfWork, cariFisiService), cari, kasa);
    }

    private static async Task<CekSenet> TahsilEdildiCekOlustur(CekSenetService servis, int cariId, decimal tutar = 500)
    {
        var cekSenet = await servis.CreateAsync(new CekSenet
        {
            BelgeTipi = BelgeTipi.Cek,
            BelgeNo = "CEK-001",
            CariId = cariId,
            VadeTarihi = DateTime.Today.AddDays(10),
            Tutar = tutar
        });
        await servis.TahsileVerAsync(cekSenet.Id);
        return cekSenet;
    }

    [Fact]
    public async Task TahsilEdildiYapAsync_TahsildekiCek_CariAlacaklandirirKasaArttirirDurumuTahsilEdildiYapar()
    {
        var (baglam, servis, cari, kasa) = await SenaryoKur();
        var cekSenet = await TahsilEdildiCekOlustur(servis, cari.Id, tutar: 500);

        await servis.TahsilEdildiYapAsync(cekSenet.Id, bankaHesabiId: null, kasaHesabiId: kasa.Id);

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelKasa = await baglam.KasaHesaplari.FindAsync(kasa.Id);
        var guncelCek = await baglam.CekSenetler.FindAsync(cekSenet.Id);
        Assert.Equal(-500, guncelCari!.Bakiye);
        Assert.Equal(1500, guncelKasa!.Bakiye);
        Assert.Equal(CekSenetDurum.TahsilEdildi, guncelCek!.Durum);
        Assert.True(baglam.CariFisleri.Single(f => f.CekSenetId == cekSenet.Id).OtomatikOlusturuldu);
    }

    [Fact]
    public async Task TahsilEdildiYapAsync_HesapSecilmemisse_HataFirlatir()
    {
        var (_, servis, cari, _) = await SenaryoKur();
        var cekSenet = await TahsilEdildiCekOlustur(servis, cari.Id);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servis.TahsilEdildiYapAsync(cekSenet.Id, bankaHesabiId: null, kasaHesabiId: null));
        Assert.Contains("banka veya kasa hesabı", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TahsilIptalEtAsync_TahsilEdilmisCek_CariVeKasaBakiyesiniTersineCevirirDurumuTahsildeYapar()
    {
        var (baglam, servis, cari, kasa) = await SenaryoKur();
        var cekSenet = await TahsilEdildiCekOlustur(servis, cari.Id);
        await servis.TahsilEdildiYapAsync(cekSenet.Id, bankaHesabiId: null, kasaHesabiId: kasa.Id);

        await servis.TahsilIptalEtAsync(cekSenet.Id);

        var guncelCari = await baglam.Cariler.FindAsync(cari.Id);
        var guncelKasa = await baglam.KasaHesaplari.FindAsync(kasa.Id);
        var guncelCek = await baglam.CekSenetler.FindAsync(cekSenet.Id);
        Assert.Equal(0, guncelCari!.Bakiye);
        Assert.Equal(1000, guncelKasa!.Bakiye);
        Assert.Equal(CekSenetDurum.Tahsilde, guncelCek!.Durum);
        Assert.True(baglam.CariFisleri.IgnoreQueryFilters().Single(f => f.CekSenetId == cekSenet.Id).IsDeleted);
    }

    [Fact]
    public async Task TahsilIptalEtAsync_TahsilEdilmemisCek_HataFirlatir()
    {
        var (_, servis, cari, _) = await SenaryoKur();
        var cekSenet = await servis.CreateAsync(new CekSenet
        {
            BelgeTipi = BelgeTipi.Cek,
            BelgeNo = "CEK-002",
            CariId = cari.Id,
            VadeTarihi = DateTime.Today.AddDays(10),
            Tutar = 500
        });

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.TahsilIptalEtAsync(cekSenet.Id));
        Assert.Contains("tahsil edildi durumundaki", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TahsilIptalEtAsync_KasaBakiyesiYetersizIse_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, servis, cari, kasa) = await SenaryoKur();
        var cekSenet = await TahsilEdildiCekOlustur(servis, cari.Id, tutar: 500);
        await servis.TahsilEdildiYapAsync(cekSenet.Id, bankaHesabiId: null, kasaHesabiId: kasa.Id);

        // Tahsilattan sonra kasadan başka bir ödeme yapılmış olsun, bakiye tahsilat tutarının altına düşsün.
        kasa.Bakiye = 200;
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.TahsilIptalEtAsync(cekSenet.Id));
        Assert.Contains("negatife düşer", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelCek = await baglam.CekSenetler.FindAsync(cekSenet.Id);
        Assert.Equal(CekSenetDurum.TahsilEdildi, guncelCek!.Durum);
    }
}
