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

    private static IHttpContextAccessor HttpContextOlustur(string? rol)
    {
        var kimlik = rol is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Role, rol)], "TestAuth");
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
}
