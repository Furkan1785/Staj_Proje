using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class CariController : Controller
{
    private readonly ICariService _cariService;

    public CariController(ICariService cariService)
    {
        _cariService = cariService;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> ListeVerisi()
    {
        var form = Request.Form;
        var draw = int.Parse(form["draw"].FirstOrDefault() ?? "1");
        var start = int.Parse(form["start"].FirstOrDefault() ?? "0");
        var length = int.Parse(form["length"].FirstOrDefault() ?? "10");
        var genelArama = form["search[value]"].FirstOrDefault();
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "0");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "asc";

        var sutunAramalari = new string?[7];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _cariService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(c => new CariListItemViewModel
        {
            Id = c.Id,
            CariKodu = c.CariKodu,
            Unvan = c.Unvan,
            CariTipiText = CariTipiMetni(c.CariTipi),
            Telefon = c.Telefon,
            EMail = c.EMail,
            Bakiye = c.Bakiye,
            IsDeleted = c.IsDeleted
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public IActionResult Ekle() => View(new CariFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(CariFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _cariService.CreateAsync(new Cari
            {
                CariKodu = vm.CariKodu,
                Unvan = vm.Unvan,
                CariTipi = vm.CariTipi,
                VergiNo = vm.VergiNo,
                Adres = vm.Adres,
                Telefon = vm.Telefon,
                EMail = vm.EMail,
                KrediLimiti = vm.KrediLimiti
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.CariKodu), ex.Message);
            return View(vm);
        }

        TempData["Basari"] = "Cari kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var cari = await _cariService.GetByIdAsync(id);
        if (cari is null) return NotFound();

        return View(new CariFormViewModel
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = cari.CariTipi,
            VergiNo = cari.VergiNo,
            Adres = cari.Adres,
            Telefon = cari.Telefon,
            EMail = cari.EMail,
            KrediLimiti = cari.KrediLimiti
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(CariFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _cariService.UpdateAsync(new Cari
            {
                Id = vm.Id,
                CariKodu = vm.CariKodu,
                Unvan = vm.Unvan,
                CariTipi = vm.CariTipi,
                VergiNo = vm.VergiNo,
                Adres = vm.Adres,
                Telefon = vm.Telefon,
                EMail = vm.EMail,
                KrediLimiti = vm.KrediLimiti
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.CariKodu), ex.Message);
            return View(vm);
        }

        TempData["Basari"] = "Cari güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(int id)
    {
        await _cariService.PasifYapAsync(id);
        TempData["Basari"] = "Cari pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(int id)
    {
        await _cariService.AktifEtAsync(id);
        TempData["Basari"] = "Cari aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    private static string CariTipiMetni(CariTipi tipi) => tipi switch
    {
        CariTipi.Musteri => "Müşteri",
        CariTipi.Tedarikci => "Tedarikçi",
        CariTipi.HerIkisi => "Her İkisi",
        _ => tipi.ToString()
    };
}
