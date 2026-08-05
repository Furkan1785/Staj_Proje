using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class AlisIrsaliyesiServiceTests
{
    private static async Task<(AppDbContext Baglam, AlisIrsaliyesiService Servis, Sube Sube, Malzeme Malzeme, Cari Cari)>
        SenaryoKur(decimal baslangicBakiye = 100)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var sube = new Sube { SubeAdi = "Merkez Şube" };
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Tedarikçi", CariTipi = CariTipi.Tedarikci };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = baslangicBakiye };
        baglam.Subeler.Add(sube);
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new AlisIrsaliyesiService(unitOfWork, onayYetkisiService), sube, malzeme, cari);
    }

    [Fact]
    public async Task OnaylaAsync_OnaylananIrsaliye_MalzemeBakiyesiniArttirir()
    {
        var (baglam, servis, sube, malzeme, cari) = await SenaryoKur(baslangicBakiye: 100);
        var irsaliye = await servis.CreateAsync(
            new AlisIrsaliyesi { CariId = cari.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new AlisIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 25 }]);

        await servis.OnaylaAsync(irsaliye.Id);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        var guncelIrsaliye = await baglam.AlisIrsaliyeleri.FindAsync(irsaliye.Id);
        Assert.Equal(125, guncelMalzeme!.Bakiye);
        Assert.Equal(BelgeDurum.Onaylandi, guncelIrsaliye!.Durum);
    }

    [Fact]
    public async Task OnaylaAsync_BeklemedeOlmayanIrsaliye_HataFirlatirVeBakiyeDegismez()
    {
        var (baglam, servis, sube, malzeme, cari) = await SenaryoKur(baslangicBakiye: 100);
        var irsaliye = await servis.CreateAsync(
            new AlisIrsaliyesi { CariId = cari.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new AlisIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 25 }]);
        await servis.OnaylaAsync(irsaliye.Id);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.OnaylaAsync(irsaliye.Id));
        Assert.Contains("beklemede", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelMalzeme = await baglam.Malzemeler.FindAsync(malzeme.Id);
        Assert.Equal(125, guncelMalzeme!.Bakiye);
    }

    [Fact]
    public async Task CreateAsync_TedarikciOlmayanCari_HataFirlatir()
    {
        var (baglam, servis, sube, malzeme, _) = await SenaryoKur();
        var musteri = new Cari { CariKodu = "C002", Unvan = "Sadece Müşteri", CariTipi = CariTipi.Musteri };
        baglam.Cariler.Add(musteri);
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.CreateAsync(
            new AlisIrsaliyesi { CariId = musteri.Id, SubeId = sube.Id, Tarih = DateTime.Today },
            [new AlisIrsaliyesiKalemi { MalzemeId = malzeme.Id, Miktar = 10 }]));

        Assert.Contains("tedarikçi", hata.Message, StringComparison.OrdinalIgnoreCase);
    }
}
