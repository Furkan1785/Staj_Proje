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

        var adminMi = _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
        if (!adminMi)
            throw new InvalidOperationException(
                $"{esik.Value.ToString("N0")} TL üstündeki bir {belgeTuru} sadece Admin tarafından onaylanabilir " +
                $"(bu belge: {tutar.ToString("N0")} TL).");
    }
}
