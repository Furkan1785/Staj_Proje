using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class BankaHesabiController : Controller
{
    private readonly IBankaHesabiService _bankaHesabiService;

    public BankaHesabiController(IBankaHesabiService bankaHesabiService)
    {
        _bankaHesabiService = bankaHesabiService;
    }

    public async Task<IActionResult> Index()
    {
        var hesaplar = await _bankaHesabiService.GetAllAsync();

        var vm = hesaplar.Select(h => new BankaHesabiListItemViewModel
        {
            Id = h.Id,
            HesapAdi = h.HesapAdi,
            BankaAdi = h.BankaAdi,
            IBAN = h.IBAN,
            Bakiye = h.Bakiye,
            ParaBirimi = h.ParaBirimi,
            IsDeleted = h.IsDeleted
        });

        return View(vm);
    }

    public IActionResult Ekle() => View(new BankaHesabiFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(BankaHesabiFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _bankaHesabiService.CreateAsync(new BankaHesabi
        {
            HesapAdi = vm.HesapAdi,
            BankaAdi = vm.BankaAdi,
            IBAN = vm.IBAN,
            ParaBirimi = vm.ParaBirimi
        });

        TempData["Basari"] = "Banka hesabı kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var hesap = await _bankaHesabiService.GetByIdAsync(id);
        if (hesap is null) return NotFound();

        return View(new BankaHesabiListItemViewModel
        {
            Id = hesap.Id,
            HesapAdi = hesap.HesapAdi,
            BankaAdi = hesap.BankaAdi,
            IBAN = hesap.IBAN,
            Bakiye = hesap.Bakiye,
            ParaBirimi = hesap.ParaBirimi,
            IsDeleted = hesap.IsDeleted
        });
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var hesap = await _bankaHesabiService.GetByIdAsync(id);
        if (hesap is null) return NotFound();

        return View(new BankaHesabiFormViewModel
        {
            Id = hesap.Id,
            HesapAdi = hesap.HesapAdi,
            BankaAdi = hesap.BankaAdi,
            IBAN = hesap.IBAN,
            ParaBirimi = hesap.ParaBirimi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(BankaHesabiFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _bankaHesabiService.UpdateAsync(new BankaHesabi
        {
            Id = vm.Id,
            HesapAdi = vm.HesapAdi,
            BankaAdi = vm.BankaAdi,
            IBAN = vm.IBAN,
            ParaBirimi = vm.ParaBirimi
        });

        TempData["Basari"] = "Banka hesabı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(int id)
    {
        await _bankaHesabiService.PasifYapAsync(id);
        TempData["Basari"] = "Banka hesabı pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(int id)
    {
        await _bankaHesabiService.AktifEtAsync(id);
        TempData["Basari"] = "Banka hesabı aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }
}
