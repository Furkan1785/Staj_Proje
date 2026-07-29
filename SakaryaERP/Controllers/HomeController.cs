using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Hem girişli hem girişsiz kullanıcıda oluşabilecek hatalar için (ör. Login sırasında
    // beklenmeyen bir hata), [AllowAnonymous] olmazsa global yetkilendirme filtresi
    // hata sayfası yerine kullanıcıyı Login'e yönlendirir — asıl hatayı gizler.
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Route açıkça belirtilmezse varsayılan {controller}/{action}/{id?} şablonu son segmenti
    // "id" olarak bağlar; "kod" adını kullanabilmek için burada açıkça tanımlanıyor.
    [AllowAnonymous]
    [Route("Home/DurumKodu/{kod:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult DurumKodu(int kod)
    {
        Response.StatusCode = kod;

        var (baslik, mesaj) = kod switch
        {
            404 => ("Sayfa Bulunamadı", "Aradığınız sayfa mevcut değil ya da taşınmış olabilir."),
            403 => ("Erişim Reddedildi", "Bu sayfaya erişim yetkiniz yok."),
            _ => ("Bir Sorun Oluştu", "İsteğiniz işlenirken beklenmeyen bir durum oluştu.")
        };

        return View(new DurumKoduViewModel { Kod = kod, Baslik = baslik, Mesaj = mesaj });
    }
}
