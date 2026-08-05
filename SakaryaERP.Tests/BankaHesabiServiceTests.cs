using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class BankaHesabiServiceTests
{
    private static (AppDbContext Baglam, BankaHesabiService Servis, BankaHesabi BankaHesabi) SenaryoKur(decimal bakiye)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var bankaHesabi = new BankaHesabi { HesapAdi = "Test Hesap", BankaAdi = "Test Banka", IBAN = "TR000000000000000000000000", Bakiye = bakiye };
        baglam.BankaHesaplari.Add(bankaHesabi);
        baglam.SaveChangesAsync().Wait();

        return (baglam, new BankaHesabiService(unitOfWork), bankaHesabi);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirOlmayanHesap_HataFirlatirVeBirSeyDegismez()
    {
        var (baglam, servis, bankaHesabi) = SenaryoKur(bakiye: 500);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => servis.PasifYapAsync(bankaHesabi.Id));
        Assert.Contains("kapanmamış bir bakiyesi", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelHesap = await baglam.BankaHesaplari.FindAsync(bankaHesabi.Id);
        Assert.False(guncelHesap!.IsDeleted);
    }

    [Fact]
    public async Task PasifYapAsync_BakiyesiSifirHesap_BasariylaPasifYapilir()
    {
        var (baglam, servis, bankaHesabi) = SenaryoKur(bakiye: 0);

        await servis.PasifYapAsync(bankaHesabi.Id);

        var guncelHesap = await baglam.BankaHesaplari.IgnoreQueryFilters().FirstAsync(b => b.Id == bankaHesabi.Id);
        Assert.True(guncelHesap.IsDeleted);
    }
}
