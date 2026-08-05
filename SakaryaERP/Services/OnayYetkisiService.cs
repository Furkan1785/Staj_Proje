using Microsoft.AspNetCore.Http;

namespace SakaryaERP.Services;

public class OnayYetkisiService : IOnayYetkisiService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public OnayYetkisiService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void YuksekTutarKontrolEt(decimal tutar, string belgeTuru)
    {
        var esik = _configuration.GetValue<decimal?>("OnayAyarlari:YuksekTutarEsigi");
        if (esik is null || tutar <= esik)
            return;

        // HttpContext yoksa bu bir web isteği değil, sistem süreci demektir (seeder,
        // arka plan job vb.) — "kullanıcı" kavramı hiç yok, bu yüzden rol bazlı onay
        // kısıtı burada uygulanmaz. Kısıt sadece gerçek bir web isteği üzerinden
        // (Admin olmayan bir kullanıcının tıklamasıyla) tetiklenen onaylarda geçerli.
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        if (!httpContext.User.IsInRole("Admin"))
            throw new InvalidOperationException(
                $"{esik.Value.ToString("N0")} TL üstündeki bir {belgeTuru} sadece Admin tarafından onaylanabilir " +
                $"(bu belge: {tutar.ToString("N0")} TL).");
    }

    public void OlusturanOnaylayamazKontrolEt(string? olusturanKullanici, string belgeTuru)
    {
        if (string.IsNullOrEmpty(olusturanKullanici))
            return;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User.IsInRole("Admin"))
            return;

        var mevcutKullanici = httpContext.User.Identity?.Name;
        if (string.Equals(mevcutKullanici, olusturanKullanici, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Bu {belgeTuru}'nu oluşturan kişi kendi belgesini onaylayamaz, başka bir yetkili onaylamalı.");
    }

    public int? MevcutKullaniciSubeId()
    {
        var subeIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("SubeId")?.Value;
        return subeIdClaim is null ? null : int.Parse(subeIdClaim);
    }

    public bool SubeErisimVarMi(int? belgeSubeId)
    {
        if (belgeSubeId is null)
            return true;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User.IsInRole("Admin"))
            return true;

        var kullaniciSubeId = MevcutKullaniciSubeId();
        return kullaniciSubeId is null || kullaniciSubeId == belgeSubeId;
    }
}
