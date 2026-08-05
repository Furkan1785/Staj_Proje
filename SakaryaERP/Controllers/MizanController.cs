using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Services;

namespace SakaryaERP.Controllers;

// Muhasebe fişleri (yevmiye kayıtları) fatura onayında otomatik üretiliyordu ama görüntüleyen
// hiçbir ekran yoktu — bu ekran o boşluğu dolduruyor: hesap bazında dönemsel Borç/Alacak/Bakiye.
[Authorize(Roles = "Admin,Muhasebe")]
public class MizanController : Controller
{
    private readonly IMizanService _mizanService;

    public MizanController(IMizanService mizanService)
    {
        _mizanService = mizanService;
    }

    public async Task<IActionResult> Ozet(DateTime? baslangic, DateTime? bitis)
    {
        var vm = await _mizanService.GetMizanAsync(baslangic, bitis);
        return View(vm);
    }
}
