using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Services;

namespace SakaryaERP.Controllers;

// Günlük arka plan bildirimi (bkz. BackgroundJobs/GunlukBildirimHostedService)
// 24 saatte bir çalışır; staj sunumunda/demoda beklemeden test edebilmek için
// Admin'e manuel tetikleme imkanı.
[Authorize(Roles = "Admin")]
public class BildirimController : Controller
{
    private readonly IBildirimService _bildirimService;

    public BildirimController(IBildirimService bildirimService)
    {
        _bildirimService = bildirimService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimdiGonder()
    {
        var sonuc = await _bildirimService.KritikDurumBildirimGonderAsync();

        TempData["Basari"] = sonuc.GonderildiMi
            ? $"Bildirim gönderildi: {sonuc.KritikStokSayisi} kritik stok, {sonuc.VadesiYaklasanCekSenetSayisi} vadesi yaklaşan çek/senet, {sonuc.GonderilenEpostaSayisi} e-posta."
            : "Gönderilecek bir kritik durum bulunamadı (kritik stok veya vadesi yaklaşan çek/senet yok).";

        return RedirectToAction("Index", "Dashboard");
    }
}
