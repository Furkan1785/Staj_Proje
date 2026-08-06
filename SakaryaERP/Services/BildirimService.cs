using System.Text;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class BildirimService : IBildirimService
{
    private readonly IMalzemeService _malzemeService;
    private readonly ICekSenetService _cekSenetService;
    private readonly IEmailSender _emailSender;
    private readonly IAdminEmailProvider _adminEmailProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BildirimService> _logger;

    public BildirimService(
        IMalzemeService malzemeService,
        ICekSenetService cekSenetService,
        IEmailSender emailSender,
        IAdminEmailProvider adminEmailProvider,
        IConfiguration configuration,
        ILogger<BildirimService> logger)
    {
        _malzemeService = malzemeService;
        _cekSenetService = cekSenetService;
        _emailSender = emailSender;
        _adminEmailProvider = adminEmailProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BildirimSonucu> KritikDurumBildirimGonderAsync()
    {
        var kritikStoklar = (await _malzemeService.GetKritikStokListesiAsync()).ToList();

        var vadeGunSayisi = _configuration.GetValue<int?>("Bildirim:VadeYaklasmaGunSayisi") ?? 3;
        var vadeSiniri = DateTime.Today.AddDays(vadeGunSayisi);
        var vadesiYaklasanlar = (await _cekSenetService.GetAllAsync())
            .Where(c => !c.IsDeleted
                && c.Durum is CekSenetDurum.Portfoyde or CekSenetDurum.Tahsilde
                && c.VadeTarihi <= vadeSiniri)
            .OrderBy(c => c.VadeTarihi)
            .ToList();

        var sonuc = new BildirimSonucu
        {
            KritikStokSayisi = kritikStoklar.Count,
            VadesiYaklasanCekSenetSayisi = vadesiYaklasanlar.Count
        };

        if (kritikStoklar.Count == 0 && vadesiYaklasanlar.Count == 0)
        {
            _logger.LogInformation("Bildirim kontrolü: kritik stok veya vadesi yaklaşan çek/senet bulunmadı, e-posta gönderilmedi.");
            return sonuc;
        }

        var htmlIcerik = HtmlIcerikOlustur(kritikStoklar, vadesiYaklasanlar, vadeGunSayisi);
        var adminEpostalari = await _adminEmailProvider.AdminEpostalariniGetirAsync();

        foreach (var email in adminEpostalari)
        {
            try
            {
                await _emailSender.SendEmailAsync(email, "TicariSistem — Kritik Durum Bildirimi", htmlIcerik);
                sonuc.GonderilenEpostaSayisi++;
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                _logger.LogWarning(ex, "Bildirim e-postası {Email} adresine gönderilemedi.", email);
            }
        }

        return sonuc;
    }

    private static string HtmlIcerikOlustur(List<Malzeme> kritikStoklar, List<CekSenet> vadesiYaklasanlar, int vadeGunSayisi)
    {
        var sb = new StringBuilder();
        sb.Append("<h2>TicariSistem — Kritik Durum Bildirimi</h2>");

        if (kritikStoklar.Count > 0)
        {
            sb.Append($"<h3>Kritik Stoktaki Malzemeler ({kritikStoklar.Count})</h3><table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");
            sb.Append("<tr><th>Malzeme Kodu</th><th>Malzeme Adı</th><th>Bakiye</th><th>Min. Stok</th></tr>");
            foreach (var m in kritikStoklar)
                sb.Append($"<tr><td>{m.MalzemeKodu}</td><td>{m.MalzemeAdi}</td><td>{m.Bakiye:N2} {m.Birim}</td><td>{m.MinStokMiktari:N2}</td></tr>");
            sb.Append("</table>");
        }

        if (vadesiYaklasanlar.Count > 0)
        {
            sb.Append($"<h3>Vadesi {vadeGunSayisi} Gün İçinde Gelen Çek/Senetler ({vadesiYaklasanlar.Count})</h3><table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");
            sb.Append("<tr><th>Belge No</th><th>Cari</th><th>Vade Tarihi</th><th>Tutar</th></tr>");
            foreach (var c in vadesiYaklasanlar)
                sb.Append($"<tr><td>{c.BelgeNo}</td><td>{c.Cari?.Unvan}</td><td>{c.VadeTarihi:dd.MM.yyyy}</td><td>{c.Tutar:N2}</td></tr>");
            sb.Append("</table>");
        }

        return sb.ToString();
    }
}
