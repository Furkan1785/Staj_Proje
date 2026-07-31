using Microsoft.Extensions.Diagnostics.HealthChecks;
using SakaryaERP.Data;

namespace SakaryaERP.HealthChecks;

public class VeritabaniHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public VeritabaniHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var baglantiVarMi = await _context.Database.CanConnectAsync(cancellationToken);
        return baglantiVarMi
            ? HealthCheckResult.Healthy("Veritabanı bağlantısı çalışıyor.")
            : HealthCheckResult.Unhealthy("Veritabanına bağlanılamıyor.");
    }
}
