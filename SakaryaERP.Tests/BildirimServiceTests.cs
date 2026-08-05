using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class KaydedenEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Html)> Gonderilenler { get; } = [];

    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        Gonderilenler.Add((toEmail, subject, htmlMessage));
        return Task.CompletedTask;
    }
}

public class SahteAdminEmailProvider(List<string> epostalar) : IAdminEmailProvider
{
    public Task<List<string>> AdminEpostalariniGetirAsync() => Task.FromResult(epostalar);
}

public class BildirimServiceTests
{
    private static IConfiguration YapilandirmaOlustur(int vadeGunSayisi = 3) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Bildirim:VadeYaklasmaGunSayisi"] = vadeGunSayisi.ToString() })
            .Build();

    private static (AppDbContext Baglam, BildirimService Servis, KaydedenEmailSender EmailSender) SenaryoKur()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);
        var emailSender = new KaydedenEmailSender();

        var onayYetkisiService = new OnayYetkisiService(new HttpContextAccessor(), new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        var servis = new BildirimService(
            new MalzemeService(unitOfWork),
            new CekSenetService(unitOfWork, new CariFisiService(unitOfWork, onayYetkisiService), onayYetkisiService),
            emailSender,
            new SahteAdminEmailProvider(["admin@sakaryaerp.com"]),
            YapilandirmaOlustur(),
            NullLogger<BildirimService>.Instance);

        return (baglam, servis, emailSender);
    }

    [Fact]
    public async Task KritikDurumBildirimGonderAsync_HicbirSeyYoksa_EpostaGondermez()
    {
        var (_, servis, emailSender) = SenaryoKur();

        var sonuc = await servis.KritikDurumBildirimGonderAsync();

        Assert.False(sonuc.GonderildiMi);
        Assert.Empty(emailSender.Gonderilenler);
    }

    [Fact]
    public async Task KritikDurumBildirimGonderAsync_KritikStokVarsa_AdminlereEpostaGonderir()
    {
        var (baglam, servis, emailSender) = SenaryoKur();
        baglam.Malzemeler.Add(new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test", Birim = "Adet", Bakiye = 2, MinStokMiktari = 10 });
        await baglam.SaveChangesAsync();

        var sonuc = await servis.KritikDurumBildirimGonderAsync();

        Assert.True(sonuc.GonderildiMi);
        Assert.Equal(1, sonuc.KritikStokSayisi);
        Assert.Single(emailSender.Gonderilenler);
        Assert.Equal("admin@sakaryaerp.com", emailSender.Gonderilenler[0].To);
        Assert.Contains("M001", emailSender.Gonderilenler[0].Html);
    }

    [Fact]
    public async Task KritikDurumBildirimGonderAsync_VadesiYakinPortfoydekiCek_Sayilir()
    {
        var (baglam, servis, emailSender) = SenaryoKur();
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = CariTipi.Musteri };
        baglam.Cariler.Add(cari);
        await baglam.SaveChangesAsync();
        baglam.CekSenetler.Add(new CekSenet
        {
            BelgeTipi = BelgeTipi.Cek,
            BelgeNo = "0001",
            CariId = cari.Id,
            VadeTarihi = DateTime.Today.AddDays(2),
            Tutar = 1000,
            Durum = CekSenetDurum.Portfoyde
        });
        await baglam.SaveChangesAsync();

        var sonuc = await servis.KritikDurumBildirimGonderAsync();

        Assert.Equal(1, sonuc.VadesiYaklasanCekSenetSayisi);
        Assert.True(sonuc.GonderildiMi);
    }

    [Fact]
    public async Task KritikDurumBildirimGonderAsync_VadesiUzakCek_Sayilmaz()
    {
        var (baglam, servis, _) = SenaryoKur();
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = CariTipi.Musteri };
        baglam.Cariler.Add(cari);
        await baglam.SaveChangesAsync();
        baglam.CekSenetler.Add(new CekSenet
        {
            BelgeTipi = BelgeTipi.Cek,
            BelgeNo = "0002",
            CariId = cari.Id,
            VadeTarihi = DateTime.Today.AddDays(30),
            Tutar = 1000,
            Durum = CekSenetDurum.Portfoyde
        });
        await baglam.SaveChangesAsync();

        var sonuc = await servis.KritikDurumBildirimGonderAsync();

        Assert.Equal(0, sonuc.VadesiYaklasanCekSenetSayisi);
        Assert.False(sonuc.GonderildiMi);
    }

    [Fact]
    public async Task KritikDurumBildirimGonderAsync_TahsilEdilmisVadesiYakinCek_Sayilmaz()
    {
        var (baglam, servis, _) = SenaryoKur();
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = CariTipi.Musteri };
        baglam.Cariler.Add(cari);
        await baglam.SaveChangesAsync();
        baglam.CekSenetler.Add(new CekSenet
        {
            BelgeTipi = BelgeTipi.Cek,
            BelgeNo = "0003",
            CariId = cari.Id,
            VadeTarihi = DateTime.Today.AddDays(1),
            Tutar = 1000,
            Durum = CekSenetDurum.TahsilEdildi
        });
        await baglam.SaveChangesAsync();

        var sonuc = await servis.KritikDurumBildirimGonderAsync();

        Assert.Equal(0, sonuc.VadesiYaklasanCekSenetSayisi);
    }
}
