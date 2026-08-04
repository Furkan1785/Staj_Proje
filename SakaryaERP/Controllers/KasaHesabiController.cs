using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
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

    public async Task<IActionResult> Detay(int id)
    {
        var hesap = await _kasaHesabiService.GetByIdAsync(id);
        if (hesap is null) return NotFound();

        return View(new KasaHesabiListItemViewModel
        {
            Id = hesap.Id,
            KasaAdi = hesap.KasaAdi,
            Bakiye = hesap.Bakiye,
            ParaBirimi = hesap.ParaBirimi,
            IsDeleted = hesap.IsDeleted
        });
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

    public async Task<IActionResult> Excel()
    {
        var hesaplar = await _kasaHesabiService.GetAllAsync();

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Kasa Hesapları");

        string[] basliklar = ["Kasa Adı", "Bakiye", "Para Birimi", "Durum"];
        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = basliklar[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var satirNo = 2;
        foreach (var h in hesaplar)
        {
            sayfa.Cell(satirNo, 1).Value = h.KasaAdi;
            sayfa.Cell(satirNo, 2).Value = h.Bakiye;
            sayfa.Cell(satirNo, 3).Value = h.ParaBirimi;
            sayfa.Cell(satirNo, 4).Value = h.IsDeleted ? "Pasif" : "Aktif";
            satirNo++;
        }
        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"kasa-hesaplari-{DateTime.Today:yyyyMMdd}.xlsx");
    }
}
