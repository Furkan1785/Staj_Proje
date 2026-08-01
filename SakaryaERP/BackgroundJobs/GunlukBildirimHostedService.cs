using SakaryaERP.Services;

namespace SakaryaERP.BackgroundJobs;

// Kritik stok ve vadesi yaklaşan çek/senet durumunu periyodik olarak kontrol edip
// Admin kullanıcılarına özet e-posta gönderir (bkz. IBildirimService). Aralık
// appsettings.json -> Bildirim:KontrolAraligiSaat ile ayarlanır (varsayılan 24 saat).
// IBildirimService Scoped olduğu için her tur için ayrı bir scope açılıyor.
public class GunlukBildirimHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GunlukBildirimHostedService> _logger;

    public GunlukBildirimHostedService(
        IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<GunlukBildirimHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var araligSaat = _configuration.GetValue<double?>("Bildirim:KontrolAraligiSaat") ?? 24;
        using var zamanlayici = new PeriodicTimer(TimeSpan.FromHours(araligSaat));

        // Önce aralık kadar beklenir, sonra kontrol edilir (uygulama her yeniden
        // başladığında — özellikle geliştirme ortamında dotnet watch ile sık sık —
        // hemen bir e-posta denemesi yapıp SMTP yapılandırılmamışsa loglara hata
        // yığmamak için). Anlık test için Dashboard'daki "Bildirimleri Şimdi Gönder"
        // butonu (bkz. BildirimController) kullanılır.
        while (await zamanlayici.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bildirimService = scope.ServiceProvider.GetRequiredService<IBildirimService>();
                var sonuc = await bildirimService.KritikDurumBildirimGonderAsync();

                if (sonuc.GonderildiMi)
                {
                    _logger.LogInformation(
                        "Günlük bildirim gönderildi: {KritikStok} kritik stok, {VadesiYaklasan} vadesi yaklaşan çek/senet, {EpostaSayisi} e-posta.",
                        sonuc.KritikStokSayisi, sonuc.VadesiYaklasanCekSenetSayisi, sonuc.GonderilenEpostaSayisi);
                }
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                _logger.LogError(ex, "Günlük bildirim kontrolü sırasında hata oluştu.");
            }
        }
    }
}
