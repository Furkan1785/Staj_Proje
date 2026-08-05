using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class KasaHesabiServiceTests
{
    private static (AppDbContext Baglam, KasaHesabiService Servis, KasaHesabi KasaHesabi) SenaryoKur(decimal bakiye)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var kasaHesabi = new KasaHesabi { KasaAdi = "Test Kasa", Bakiye = bakiye };
        baglam.KasaHesaplari.Add(kasaHesabi);
        baglam.SaveChangesAsync().Wait();

        return (baglam, new KasaHesabiService(unitOfWork), kasaHesabi);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirOlmayanHesap_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, servis, kasaHesabi) = SenaryoKur(bakiye: 500);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.PasifYapAsync(kasaHesabi.Id));
        Assert.Contains("kapanmamış bir bakiyesi", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelHesap = await baglam.KasaHesaplari.FindAsync(kasaHesabi.Id);
        Assert.False(guncelHesap!.IsDeleted);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirHesap_BasariylaPasifYapilir()
    {
        var (baglam, servis, kasaHesabi) = SenaryoKur(bakiye: 0);

        await servis.PasifYapAsync(kasaHesabi.Id);

        var guncelHesap = await baglam.KasaHesaplari.IgnoreQueryFilters().FirstAsync(k => k.Id == kasaHesabi.Id);
        Assert.True(guncelHesap.IsDeleted);
    }
}
