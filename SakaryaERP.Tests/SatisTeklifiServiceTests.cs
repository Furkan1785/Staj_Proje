using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;

namespace SakaryaERP.Tests;

public class SatisTeklifiServiceTests
{
    private static async Task<(AppDbContext Baglam, SatisTeklifiService Servis, Cari Cari, Malzeme Malzeme, MusteriTalebi Talep)> SenaryoKur()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = 100 };
        var talep = new MusteriTalebi { TalepNo = "MT-000001", CariId = cari.Id, Tarih = DateTime.Today, Durum = TalepDurum.Yeni };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);
        baglam.MusteriTalepleri.Add(talep);
        await baglam.SaveChangesAsync();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return (baglam, new SatisTeklifiService(unitOfWork, onayYetkisiService), cari, malzeme, talep);
    }

    private static List<SatisTeklifiKalemi> Kalemler(int malzemeId) =>
        [new SatisTeklifiKalemi { MalzemeId = malzemeId, Miktar = 10, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }];

    [Fact]
    public async Task CreateAsync_TaleptenTeklifOlusturulunca_TalepTamamlandiOlur()
    {
        var (baglam, servis, cari, malzeme, talep) = await SenaryoKur();

        await servis.CreateAsync(
            new SatisTeklifi { CariId = cari.Id, MusteriTalebiId = talep.Id, Tarih = DateTime.Today },
            Kalemler(malzeme.Id));

        var guncelTalep = await baglam.MusteriTalepleri.FindAsync(talep.Id);
        Assert.Equal(TalepDurum.Tamamlandi, guncelTalep!.Durum);
    }

    [Fact]
    public async Task IptalEtAsync_TekTeklifIptalEdilince_TalepIslenıyoraDonupYenidenTeklifUretilebilir()
    {
        var (baglam, servis, cari, malzeme, talep) = await SenaryoKur();
        var teklif = await servis.CreateAsync(
            new SatisTeklifi { CariId = cari.Id, MusteriTalebiId = talep.Id, Tarih = DateTime.Today },
            Kalemler(malzeme.Id));

        await servis.IptalEtAsync(teklif.Id);

        var guncelTalep = await baglam.MusteriTalepleri.FindAsync(talep.Id);
        Assert.Equal(TalepDurum.Isleniyor, guncelTalep!.Durum);

        // Talep artık Isleniyor olduğu için aynı talepten ikinci bir teklif üretilebilmeli.
        var ikinciTeklif = await servis.CreateAsync(
            new SatisTeklifi { CariId = cari.Id, MusteriTalebiId = talep.Id, Tarih = DateTime.Today },
            Kalemler(malzeme.Id));
        Assert.Equal("ST-000002", ikinciTeklif.TeklifNo);
    }

    [Fact]
    public async Task IptalEtAsync_AyniTaleptenBaskaAktifTeklifVarsa_TalepTamamlandiKalir()
    {
        var (baglam, servis, cari, malzeme, talep) = await SenaryoKur();
        var teklif1 = await servis.CreateAsync(
            new SatisTeklifi { CariId = cari.Id, MusteriTalebiId = talep.Id, Tarih = DateTime.Today },
            Kalemler(malzeme.Id));

        // Talep Tamamlandi olduğu için normalde ikinci teklif üretilemez; senaryoyu kurmak için
        // durumu elle Isleniyor'a alıp ikinci teklifi oluşturuyoruz.
        talep.Durum = TalepDurum.Isleniyor;
        await baglam.SaveChangesAsync();
        var teklif2 = await servis.CreateAsync(
            new SatisTeklifi { CariId = cari.Id, MusteriTalebiId = talep.Id, Tarih = DateTime.Today },
            Kalemler(malzeme.Id));

        await servis.IptalEtAsync(teklif1.Id);

        var guncelTalep = await baglam.MusteriTalepleri.FindAsync(talep.Id);
        Assert.Equal(TalepDurum.Tamamlandi, guncelTalep!.Durum);
    }
}
