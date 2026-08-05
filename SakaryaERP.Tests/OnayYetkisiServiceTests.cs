using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SakaryaERP.Services;
using Xunit;

namespace SakaryaERP.Tests;

public class OnayYetkisiServiceTests
{
    private static IConfiguration YapilandirmaOlustur(decimal? esik) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(esik is null
                ? []
                : new Dictionary<string, string?> { ["OnayAyarlari:YuksekTutarEsigi"] = esik.Value.ToString() })
            .Build();

    private static IHttpContextAccessor HttpContextOlustur(string? rol, int? subeId = null, string? kullaniciAdi = null)
    {
        List<Claim> claimler = [];
        if (rol is not null) claimler.Add(new Claim(ClaimTypes.Role, rol));
        if (subeId is not null) claimler.Add(new Claim("SubeId", subeId.Value.ToString()));
        if (kullaniciAdi is not null) claimler.Add(new Claim(ClaimTypes.Name, kullaniciAdi));
        var kimlik = claimler.Count == 0 ? new ClaimsIdentity() : new ClaimsIdentity(claimler, "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(kimlik) };
        return new HttpContextAccessor { HttpContext = context };
    }

    [Fact]
    public void YuksekTutarKontrolEt_EsikAltindaTutar_AdminOlmayanIcinDeSorunOlmaz()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis"), YapilandirmaOlustur(50000));

        var hata = Record.Exception(() => servis.YuksekTutarKontrolEt(10000, "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void YuksekTutarKontrolEt_EsikUstuTutar_AdminOlmayanKullaniciyaHataFirlatir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis"), YapilandirmaOlustur(50000));

        var hata = Assert.Throws<InvalidOperationException>(() => servis.YuksekTutarKontrolEt(75000, "Satış Faturası"));

        Assert.Contains("sadece Admin", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void YuksekTutarKontrolEt_EsikUstuTutar_AdminIcinSorunOlmaz()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Admin"), YapilandirmaOlustur(50000));

        var hata = Record.Exception(() => servis.YuksekTutarKontrolEt(75000, "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void YuksekTutarKontrolEt_EsikYapilandirilmamissa_HicbirZamanEngellemez()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis"), YapilandirmaOlustur(null));

        var hata = Record.Exception(() => servis.YuksekTutarKontrolEt(1_000_000, "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void YuksekTutarKontrolEt_HttpContextYokken_SistemSureciSayilirEngellenmez()
    {
        // HttpContext yokluğu bir web isteği dışında çalışıldığı anlamına gelir (seeder,
        // arka plan job vb.) — "kullanıcı" kavramı olmadığı için rol kısıtı uygulanmaz.
        var servis = new OnayYetkisiService(new HttpContextAccessor(), YapilandirmaOlustur(50000));

        var hata = Record.Exception(() => servis.YuksekTutarKontrolEt(75000, "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void SubeErisimVarMi_BelgeSubesizse_HerkeseAcik()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 1), YapilandirmaOlustur(null));

        Assert.True(servis.SubeErisimVarMi(null));
    }

    [Fact]
    public void SubeErisimVarMi_FarkliSubedekiKullanici_ErisemezFalseDoner()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 1), YapilandirmaOlustur(null));

        Assert.False(servis.SubeErisimVarMi(2));
    }

    [Fact]
    public void SubeErisimVarMi_AyniSubedekiKullanici_Erisir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 1), YapilandirmaOlustur(null));

        Assert.True(servis.SubeErisimVarMi(1));
    }

    [Fact]
    public void SubeErisimVarMi_Admin_HerZamanErisir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Admin", subeId: 1), YapilandirmaOlustur(null));

        Assert.True(servis.SubeErisimVarMi(2));
    }

    [Fact]
    public void SubeErisimVarMi_SubesizMerkezKullanici_HerSubeyeErisir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Muhasebe"), YapilandirmaOlustur(null));

        Assert.True(servis.SubeErisimVarMi(2));
    }

    [Fact]
    public void MevcutKullaniciSubeId_ClaimVarsa_DegeriDoner()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 3), YapilandirmaOlustur(null));

        Assert.Equal(3, servis.MevcutKullaniciSubeId());
    }

    [Fact]
    public void MevcutKullaniciSubeId_ClaimYoksa_NullDoner()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Muhasebe"), YapilandirmaOlustur(null));

        Assert.Null(servis.MevcutKullaniciSubeId());
    }

    [Fact]
    public void OlusturanOnaylayamazKontrolEt_OlusturanKendiOnaylamayaCalisir_HataFirlatir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", kullaniciAdi: "satis@test.com"), YapilandirmaOlustur(null));

        var hata = Assert.Throws<InvalidOperationException>(
            () => servis.OlusturanOnaylayamazKontrolEt("satis@test.com", "Satış Faturası"));

        Assert.Contains("kendi belgesini onaylayamaz", hata.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OlusturanOnaylayamazKontrolEt_BaskaKullaniciOnaylar_SorunOlmaz()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", kullaniciAdi: "muhasebe@test.com"), YapilandirmaOlustur(null));

        var hata = Record.Exception(() => servis.OlusturanOnaylayamazKontrolEt("satis@test.com", "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void OlusturanOnaylayamazKontrolEt_Admin_KendiBelgesiniDeOnaylayabilir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Admin", kullaniciAdi: "admin@test.com"), YapilandirmaOlustur(null));

        var hata = Record.Exception(() => servis.OlusturanOnaylayamazKontrolEt("admin@test.com", "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void OlusturanOnaylayamazKontrolEt_OlusturanNull_KontrolUygulanmaz()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", kullaniciAdi: "satis@test.com"), YapilandirmaOlustur(null));

        var hata = Record.Exception(() => servis.OlusturanOnaylayamazKontrolEt(null, "Satış Faturası"));

        Assert.Null(hata);
    }

    [Fact]
    public void EfektifRaporSubesi_SubeliKullaniciBaskaSubeIster_KendiSubesineSabitlenir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 1), YapilandirmaOlustur(null));

        Assert.Equal(1, servis.EfektifRaporSubesi(2));
    }

    [Fact]
    public void EfektifRaporSubesi_SubeliKullaniciTumSubeleriIster_KendiSubesineSabitlenir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Satis", subeId: 1), YapilandirmaOlustur(null));

        Assert.Equal(1, servis.EfektifRaporSubesi(null));
    }

    [Fact]
    public void EfektifRaporSubesi_Admin_IstenenDegerAynenKullanilir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Admin", subeId: 1), YapilandirmaOlustur(null));

        Assert.Equal(2, servis.EfektifRaporSubesi(2));
        Assert.Null(servis.EfektifRaporSubesi(null));
    }

    [Fact]
    public void EfektifRaporSubesi_SubesizMerkezKullanici_IstenenDegerAynenKullanilir()
    {
        var servis = new OnayYetkisiService(HttpContextOlustur("Muhasebe"), YapilandirmaOlustur(null));

        Assert.Equal(2, servis.EfektifRaporSubesi(2));
        Assert.Null(servis.EfektifRaporSubesi(null));
    }
}
