using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class KasaHesabiController : Controller
{
    private readonly IKasaHesabiService _kasaHesabiService;

    public KasaHesabiController(IKasaHesabiService kasaHesabiService)
    {
        _kasaHesabiService = kasaHesabiService;
    }

    public async Task<IActionResult> Index()
    {
        var hesaplar = await _kasaHesabiService.GetAllAsync();

        var vm = hesaplar.Select(h => new KasaHesabiListItemViewModel
        {
            Id = h.Id,
            KasaAdi = h.KasaAdi,
            Bakiye = h.Bakiye,
            ParaBirimi = h.ParaBirimi,
            IsDeleted = h.IsDeleted
        });

        return View(vm);
    }

    public IActionResult Ekle() => View(new KasaHesabiFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(KasaHesabiFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _kasaHesabiService.CreateAsync(new KasaHesabi
        {
            KasaAdi = vm.KasaAdi,
            ParaBirimi = vm.ParaBirimi
        });

        TempData["Basari"] = "Kasa hesabı kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var hesap = await _kasaHesabiService.GetByIdAsync(id);
        if (hesap is null) return NotFound();

        return View(new KasaHesabiFormViewModel
        {
            Id = hesap.Id,
            KasaAdi = hesap.KasaAdi,
            ParaBirimi = hesap.ParaBirimi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(KasaHesabiFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _kasaHesabiService.UpdateAsync(new KasaHesabi
        {
            Id = vm.Id,
            KasaAdi = vm.KasaAdi,
            ParaBirimi = vm.ParaBirimi
        });

        TempData["Basari"] = "Kasa hesabı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(int id)
    {
        await _kasaHesabiService.PasifYapAsync(id);
        TempData["Basari"] = "Kasa hesabı pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(int id)
    {
        await _kasaHesabiService.AktifEtAsync(id);
        TempData["Basari"] = "Kasa hesabı aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }
}
