using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class SubeController : Controller
{
    private readonly ISubeService _subeService;

    public SubeController(ISubeService subeService)
    {
        _subeService = subeService;
    }

    public async Task<IActionResult> Index()
    {
        var subeler = await _subeService.GetAllAsync();

        var vm = subeler.Select(s => new SubeListItemViewModel
        {
            Id = s.Id,
            SubeAdi = s.SubeAdi,
            Adres = s.Adres,
            IsDeleted = s.IsDeleted
        });

        return View(vm);
    }

    public IActionResult Ekle() => View(new SubeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(SubeFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _subeService.CreateAsync(new Sube
        {
            SubeAdi = vm.SubeAdi,
            Adres = vm.Adres
        });

        TempData["Basari"] = "Şube kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var sube = await _subeService.GetByIdAsync(id);
        if (sube is null) return NotFound();

        return View(new SubeFormViewModel
        {
            Id = sube.Id,
            SubeAdi = sube.SubeAdi,
            Adres = sube.Adres
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(SubeFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _subeService.UpdateAsync(new Sube
        {
            Id = vm.Id,
            SubeAdi = vm.SubeAdi,
            Adres = vm.Adres
        });

        TempData["Basari"] = "Şube güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(int id)
    {
        await _subeService.PasifYapAsync(id);
        TempData["Basari"] = "Şube pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(int id)
    {
        await _subeService.AktifEtAsync(id);
        TempData["Basari"] = "Şube aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }
}
