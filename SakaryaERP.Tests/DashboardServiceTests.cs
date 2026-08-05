using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;
using Xunit;

namespace SakaryaERP.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_OnaylanmisSatisFaturasi_BrutKariNetSatisEksiMaliyetOlarakHesaplar()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = 100, AlisFiyati = 30 };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);
        foreach (var (kod, ad) in new[] { ("120", "Alıcılar"), ("600", "Yurtiçi Satışlar"), ("391", "Hesaplanan KDV"), ("621", "Satılan Ticari Mallar Maliyeti"), ("153", "Ticari Mallar") })
            baglam.HesapPlani.Add(new HesapPlani { HesapKodu = kod, HesapAdi = ad, HesapTipi = HesapTipi.Aktif });
        await baglam.SaveChangesAsync();

        var satisFaturasiServis = new SatisFaturasiService(unitOfWork, onayYetkisiService);
        var fatura = await satisFaturasiServis.CreateAsync(
            new SatisFaturasi { CariId = cari.Id, Tarih = DateTime.Today, Kalemler = [] },
            [new SatisFaturasiKalemi { MalzemeId = malzeme.Id, Miktar = 10, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }]);
        await satisFaturasiServis.OnaylaAsync(fatura.Id);

        var alisFaturasiServis = new AlisFaturasiService(unitOfWork, onayYetkisiService);
        var malzemeServis = new MalzemeService(unitOfWork);
        var dashboardServis = new DashboardService(satisFaturasiServis, alisFaturasiServis, malzemeServis);

        var vm = await dashboardServis.GetDashboardAsync(DashboardAralik.TumZamanlar, []);

        // Toplam (KDV dahil): 10 * 50 * 1.20 = 600. Net satış (KDV hariç): 500. Maliyet: 10 * 30
        // (Malzeme.AlisFiyati'nin onay anındaki anlık görüntüsü) = 300. Brüt kar: 500 - 300 = 200.
        Assert.Equal(600, vm.GenelSatisToplamiTRY);
        Assert.Equal(200, vm.BrutKar);
    }
}
