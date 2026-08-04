using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class SatisSiparisiServiceTests
{
    private static async Task<(AppDbContext Baglam, UnitOfWork UnitOfWork, Cari Cari, Malzeme Malzeme, SatisTeklifi Teklif)>
        TemelVeriKur(BelgeDurum teklifDurumu = BelgeDurum.Onaylandi, DateTime? gecerlilikTarihi = null)
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var unitOfWork = new UnitOfWork(baglam);

        var cari = new Cari { CariKodu = "C001", Unvan = "Test Müşteri", CariTipi = CariTipi.Musteri };
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Test Malzemesi", Birim = "Adet", Bakiye = 100 };
        baglam.Cariler.Add(cari);
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        var teklif = new SatisTeklifi
        {
            TeklifNo = "ST-000001",
            CariId = cari.Id,
            Tarih = DateTime.Today,
            GecerlilikTarihi = gecerlilikTarihi ?? DateTime.Today.AddDays(10),
            Durum = teklifDurumu,
            Kalemler = [new SatisTeklifiKalemi { MalzemeId = malzeme.Id, Miktar = 10, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }]
        };
        baglam.SatisTeklifleri.Add(teklif);
        await baglam.SaveChangesAsync();

        return (baglam, unitOfWork, cari, malzeme, teklif);
    }

    private static async Task<(AppDbContext Baglam, SatisSiparisiService Servis, Cari Cari, Malzeme Malzeme, SatisTeklifi Teklif)>
        SenaryoKur(BelgeDurum teklifDurumu = BelgeDurum.Onaylandi, DateTime? gecerlilikTarihi = null)
    {
        var (baglam, unitOfWork, cari, malzeme, teklif) = await TemelVeriKur(teklifDurumu, gecerlilikTarihi);
        return (baglam, ServisOlustur(unitOfWork, kullaniciAdi: null, rol: null), cari, malzeme, teklif);
    }

    private static SatisSiparisi YeniSiparis(int cariId, int? teklifId) => new()
    {
        CariId = cariId,
        SatisTeklifiId = teklifId,
        Tarih = DateTime.Today
    };

    private static List<SatisSiparisiKalemi> Kalemler(int malzemeId) =>
        [new SatisSiparisiKalemi { MalzemeId = malzemeId, Miktar = 10, BirimFiyat = 50, KdvOrani = 20, Iskonto = 0 }];

    private static SatisSiparisiService ServisOlustur(UnitOfWork unitOfWork, string? kullaniciAdi, string? rol)
    {
        var kimlik = kullaniciAdi is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, kullaniciAdi), .. rol is null ? [] : new[] { new Claim(ClaimTypes.Role, rol) }], "TestAuth");
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(kimlik) } };
        var onayYetkisiService = new OnayYetkisiService(accessor, new ConfigurationBuilder().AddInMemoryCollection([]).Build());
        return new SatisSiparisiService(unitOfWork, onayYetkisiService);
    }

    [Fact]
    public async Task CreateAsync_OnayliVeSuresiGecmemisTeklif_BasariylaOlusur()
    {
        var (_, servis, cari, malzeme, teklif) = await SenaryoKur();

        var siparis = await servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));

        Assert.Equal("SS-000001", siparis.SiparisNo);
    }

    [Fact]
    public async Task CreateAsync_SuresiGecmisTeklif_HataFirlatir()
    {
        var (_, servis, cari, malzeme, teklif) = await SenaryoKur(gecerlilikTarihi: DateTime.Today.AddDays(-1));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id)));

        Assert.Contains("geçerlilik süresi", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_AyniTekliftenIkinciSiparis_HataFirlatir()
    {
        var (_, servis, cari, malzeme, teklif) = await SenaryoKur();

        await servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id)));

        Assert.Contains("zaten bir sipariş", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_IlkSiparisIptalEdildiyseAyniTekliftenYeniSiparis_BasariylaOlusur()
    {
        var (_, servis, cari, malzeme, teklif) = await SenaryoKur();

        var ilkSiparis = await servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));
        await servis.IptalEtAsync(ilkSiparis.Id);

        var yeniSiparis = await servis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));

        Assert.Equal("SS-000002", yeniSiparis.SiparisNo);
    }

    [Fact]
    public async Task OnaylaAsync_OlusturanKendiSiparisiniOnaylayamaz_HataFirlatirVeDurumDegismez()
    {
        var (baglam, unitOfWork, cari, malzeme, teklif) = await TemelVeriKur();
        var olusturanServis = ServisOlustur(unitOfWork, kullaniciAdi: "satiseleman1", rol: "Satis");
        var siparis = await olusturanServis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));
        siparis.CreatedBy = "satiseleman1";
        await baglam.SaveChangesAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => olusturanServis.OnaylaAsync(siparis.Id));
        Assert.Contains("kendi belgesini onaylayamaz", hata.Message, StringComparison.OrdinalIgnoreCase);

        var guncelSiparis = await baglam.SatisSiparisleri.FindAsync(siparis.Id);
        Assert.Equal(BelgeDurum.Beklemede, guncelSiparis!.Durum);
    }

    [Fact]
    public async Task OnaylaAsync_BaskaKullaniciOnaylarsa_BasariylaOnaylanir()
    {
        var (baglam, unitOfWork, cari, malzeme, teklif) = await TemelVeriKur();
        var olusturanServis = ServisOlustur(unitOfWork, kullaniciAdi: "satiseleman1", rol: "Satis");
        var siparis = await olusturanServis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));
        siparis.CreatedBy = "satiseleman1";
        await baglam.SaveChangesAsync();

        var baskaServis = ServisOlustur(unitOfWork, kullaniciAdi: "satiseleman2", rol: "Satis");
        await baskaServis.OnaylaAsync(siparis.Id);

        var guncelSiparis = await baglam.SatisSiparisleri.FindAsync(siparis.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelSiparis!.Durum);
    }

    [Fact]
    public async Task OnaylaAsync_AdminKendiSiparisiniDeOnaylayabilir()
    {
        var (baglam, unitOfWork, cari, malzeme, teklif) = await TemelVeriKur();
        var adminServis = ServisOlustur(unitOfWork, kullaniciAdi: "admin1", rol: "Admin");
        var siparis = await adminServis.CreateAsync(YeniSiparis(cari.Id, teklif.Id), Kalemler(malzeme.Id));
        siparis.CreatedBy = "admin1";
        await baglam.SaveChangesAsync();

        await adminServis.OnaylaAsync(siparis.Id);

        var guncelSiparis = await baglam.SatisSiparisleri.FindAsync(siparis.Id);
        Assert.Equal(BelgeDurum.Onaylandi, guncelSiparis!.Durum);
    }
}
