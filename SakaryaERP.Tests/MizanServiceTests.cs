using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class MizanServiceTests
{
    [Fact]
    public async Task GetMizanAsync_BirdenFazlaFis_HesapBazindaGrupluToplarVeBakiyeHesaplar()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var alicilar = new HesapPlani { HesapKodu = "120", HesapAdi = "Alıcılar", HesapTipi = HesapTipi.Aktif };
        var satislar = new HesapPlani { HesapKodu = "600", HesapAdi = "Yurtiçi Satışlar", HesapTipi = HesapTipi.Gelir };
        baglam.HesapPlani.AddRange(alicilar, satislar);
        await baglam.SaveChangesAsync();

        baglam.MuhasebeFisleri.Add(new MuhasebeFisi
        {
            FisNo = "MF-000001",
            Tarih = new DateTime(2026, 3, 10),
            Kalemler =
            [
                new MuhasebeFisiKalemi { HesapPlaniId = alicilar.Id, Borc = 600, Alacak = 0 },
                new MuhasebeFisiKalemi { HesapPlaniId = satislar.Id, Borc = 0, Alacak = 600 }
            ]
        });
        baglam.MuhasebeFisleri.Add(new MuhasebeFisi
        {
            FisNo = "MF-000002",
            Tarih = new DateTime(2026, 3, 20),
            Kalemler =
            [
                new MuhasebeFisiKalemi { HesapPlaniId = alicilar.Id, Borc = 300, Alacak = 0 },
                new MuhasebeFisiKalemi { HesapPlaniId = satislar.Id, Borc = 0, Alacak = 300 }
            ]
        });
        await baglam.SaveChangesAsync();

        var servis = new MizanService(unitOfWork);
        var vm = await servis.GetMizanAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));

        Assert.Equal(2, vm.Satirlar.Count);
        var alicilarSatiri = vm.Satirlar.Single(s => s.HesapKodu == "120");
        Assert.Equal(900, alicilarSatiri.ToplamBorc);
        Assert.Equal(0, alicilarSatiri.ToplamAlacak);
        Assert.Equal(900, alicilarSatiri.BorcBakiyesi);

        var satislarSatiri = vm.Satirlar.Single(s => s.HesapKodu == "600");
        Assert.Equal(900, satislarSatiri.ToplamAlacak);
        Assert.Equal(900, satislarSatiri.AlacakBakiyesi);

        Assert.Equal(vm.ToplamBorc, vm.ToplamAlacak);
    }

    [Fact]
    public async Task GetMizanAsync_TarihAraligiDisindakiFis_Sayilmaz()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var hesap = new HesapPlani { HesapKodu = "100", HesapAdi = "Kasa", HesapTipi = HesapTipi.Aktif };
        baglam.HesapPlani.Add(hesap);
        await baglam.SaveChangesAsync();

        baglam.MuhasebeFisleri.Add(new MuhasebeFisi
        {
            FisNo = "MF-000001",
            Tarih = new DateTime(2025, 1, 1),
            Kalemler = [new MuhasebeFisiKalemi { HesapPlaniId = hesap.Id, Borc = 500, Alacak = 0 }]
        });
        await baglam.SaveChangesAsync();

        var servis = new MizanService(unitOfWork);
        var vm = await servis.GetMizanAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        Assert.Empty(vm.Satirlar);
    }

    [Fact]
    public async Task GetMizanAsync_IptalEdilmisFaturaninYevmiyeKaydi_MizanaDahilEdilmez()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var alicilar = new HesapPlani { HesapKodu = "120", HesapAdi = "Alıcılar", HesapTipi = HesapTipi.Aktif };
        baglam.HesapPlani.Add(alicilar);
        await baglam.SaveChangesAsync();

        // İptal edilen bir faturanın yevmiye kaydı SatisFaturasiService.IptalEtAsync tarafından
        // soft-delete edilir (IsDeleted=true) — CariFisi/MuhasebeFisi silinmez, sadece işaretlenir.
        // Önce normal ekleyip sonra ayrı bir SaveChangesAsync ile işaretliyoruz: AppDbContext.
        // SaveChangesAsync, Added durumundaki kayıtlarda IsDeleted'i baştan false'a sıfırlıyor
        // (yanlışlıkla silinmiş kayıt oluşturulmasını engellemek için), bu yüzden Add sırasında
        // IsDeleted=true vermek gerçek senaryoyu simüle etmez.
        var fis = new MuhasebeFisi
        {
            FisNo = "MF-000001",
            Tarih = new DateTime(2026, 3, 10),
            Kalemler = [new MuhasebeFisiKalemi { HesapPlaniId = alicilar.Id, Borc = 600, Alacak = 0 }]
        };
        baglam.MuhasebeFisleri.Add(fis);
        await baglam.SaveChangesAsync();

        fis.IsDeleted = true;
        baglam.MuhasebeFisleri.Update(fis);
        await baglam.SaveChangesAsync();

        var servis = new MizanService(unitOfWork);
        var vm = await servis.GetMizanAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));

        Assert.Empty(vm.Satirlar);
    }

    [Fact]
    public async Task GetMizanAsync_BaslangicBitistenSonraysa_HataFirlatir()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var servis = new MizanService(unitOfWork);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servis.GetMizanAsync(new DateTime(2026, 12, 31), new DateTime(2026, 1, 1)));

        Assert.Contains("başlangıç", hata.Message, StringComparison.OrdinalIgnoreCase);
    }
}
