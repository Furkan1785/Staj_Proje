using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class CekSenetController : Controller
{
    private readonly ICekSenetService _cekSenetService;
    private readonly ICariService _cariService;
    private readonly IBankaHesabiService _bankaHesabiService;
    private readonly IKasaHesabiService _kasaHesabiService;

    public CekSenetController(
        ICekSenetService cekSenetService,
        ICariService cariService,
        IBankaHesabiService bankaHesabiService,
        IKasaHesabiService kasaHesabiService)
    {
        _cekSenetService = cekSenetService;
        _cariService = cariService;
        _bankaHesabiService = bankaHesabiService;
        _kasaHesabiService = kasaHesabiService;
    }

    public async Task<IActionResult> Index()
    {
        var bankaHesaplari = await _bankaHesabiService.GetAllAsync();
        var kasaHesaplari = await _kasaHesabiService.GetAllAsync();

        return View(new CekSenetIndexViewModel
        {
            BankaHesabiListesi = bankaHesaplari.Where(b => !b.IsDeleted)
                .Select(b => new SelectListItem($"{b.HesapAdi} ({b.BankaAdi})", b.Id.ToString())),
            KasaHesabiListesi = kasaHesaplari.Where(k => !k.IsDeleted)
                .Select(k => new SelectListItem(k.KasaAdi, k.Id.ToString()))
        });
    }

    [HttpPost]
    public async Task<IActionResult> ListeVerisi()
    {
        var form = Request.Form;
        var draw = int.Parse(form["draw"].FirstOrDefault() ?? "1");
        var start = int.Parse(form["start"].FirstOrDefault() ?? "0");
        var length = int.Parse(form["length"].FirstOrDefault() ?? "10");
        var genelArama = form["search[value]"].FirstOrDefault();
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "3");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "asc";

        var sutunAramalari = new string?[8];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _cekSenetService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(c => new CekSenetListItemViewModel
        {
            Id = c.Id,
            BelgeTipiText = BelgeTipiMetni(c.BelgeTipi),
            BelgeNo = c.BelgeNo,
            CariUnvan = c.Cari.Unvan,
            VadeTarihi = c.VadeTarihi,
            Tutar = c.Tutar,
            BankaAdi = c.BankaAdi,
            SubeAdi = c.SubeAdi,
            Durum = c.Durum.ToString(),
            DurumText = DurumMetni(c.Durum)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new CekSenetFormViewModel();
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(CekSenetFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            await _cekSenetService.CreateAsync(new CekSenet
            {
                BelgeTipi = vm.BelgeTipi,
                BelgeNo = vm.BelgeNo,
                CariId = vm.CariId,
                CiroBilgisi = vm.CiroBilgisi,
                VadeTarihi = vm.VadeTarihi,
                Tutar = vm.Tutar,
                BankaAdi = vm.BankaAdi,
                SubeAdi = vm.SubeAdi
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Çek/Senet kaydı eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var cekSenet = await _cekSenetService.GetByIdAsync(id);
        if (cekSenet is null)
            return NotFound();

        var cari = await _cariService.GetByIdAsync(cekSenet.CariId);
        var bankaHesaplari = await _bankaHesabiService.GetAllAsync();
        var kasaHesaplari = await _kasaHesabiService.GetAllAsync();

        return View(new CekSenetDetayViewModel
        {
            Id = cekSenet.Id,
            BelgeTipi = cekSenet.BelgeTipi,
            BelgeTipiText = BelgeTipiMetni(cekSenet.BelgeTipi),
            BelgeNo = cekSenet.BelgeNo,
            CariUnvan = cari?.Unvan ?? "-",
            CiroBilgisi = cekSenet.CiroBilgisi,
            VadeTarihi = cekSenet.VadeTarihi,
            Tutar = cekSenet.Tutar,
            BankaAdi = cekSenet.BankaAdi,
            SubeAdi = cekSenet.SubeAdi,
            Durum = cekSenet.Durum,
            DurumText = DurumMetni(cekSenet.Durum),
            BankaHesabiListesi = bankaHesaplari.Where(b => !b.IsDeleted)
                .Select(b => new SelectListItem($"{b.HesapAdi} ({b.BankaAdi})", b.Id.ToString())),
            KasaHesabiListesi = kasaHesaplari.Where(k => !k.IsDeleted)
                .Select(k => new SelectListItem(k.KasaAdi, k.Id.ToString()))
        });
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var cekSenet = await _cekSenetService.GetByIdAsync(id);
        if (cekSenet is null)
            return NotFound();

        if (cekSenet.Durum != CekSenetDurum.Portfoyde)
        {
            TempData["Hata"] = $"Durumu '{DurumMetni(cekSenet.Durum)}' olan bir çek/senet düzenlenemez.";
            return RedirectToAction(nameof(Detay), new { id });
        }

        var vm = new CekSenetFormViewModel
        {
            Id = cekSenet.Id,
            BelgeTipi = cekSenet.BelgeTipi,
            BelgeNo = cekSenet.BelgeNo,
            CariId = cekSenet.CariId,
            CiroBilgisi = cekSenet.CiroBilgisi,
            VadeTarihi = cekSenet.VadeTarihi,
            Tutar = cekSenet.Tutar,
            BankaAdi = cekSenet.BankaAdi,
            SubeAdi = cekSenet.SubeAdi
        };
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(CekSenetFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            await _cekSenetService.UpdateAsync(new CekSenet
            {
                Id = vm.Id,
                BelgeTipi = vm.BelgeTipi,
                BelgeNo = vm.BelgeNo,
                CariId = vm.CariId,
                CiroBilgisi = vm.CiroBilgisi,
                VadeTarihi = vm.VadeTarihi,
                Tutar = vm.Tutar,
                BankaAdi = vm.BankaAdi,
                SubeAdi = vm.SubeAdi
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Çek/Senet kaydı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TahsileVer(int id, bool returnToDetay = false)
    {
        try
        {
            await _cekSenetService.TahsileVerAsync(id);
            TempData["Basari"] = "Belge tahsile verildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CiroEt(int id, string ciroBilgisi, bool returnToDetay = false)
    {
        try
        {
            await _cekSenetService.CiroEtAsync(id, ciroBilgisi);
            TempData["Basari"] = "Belge ciro edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TahsilEdildi(int id, int? bankaHesabiId, int? kasaHesabiId, bool returnToDetay = false)
    {
        try
        {
            await _cekSenetService.TahsilEdildiYapAsync(id, bankaHesabiId, kasaHesabiId);
            TempData["Basari"] = "Belge tahsil edildi, cari ve hesap bakiyesi güncellendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Karsiliksiz(int id, bool returnToDetay = false)
    {
        try
        {
            await _cekSenetService.KarsiliksizYapAsync(id);
            TempData["Basari"] = "Belge karşılıksız olarak işaretlendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    private async Task DoldurListeler(CekSenetFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.CariListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Musteri or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));
    }

    private static string BelgeTipiMetni(BelgeTipi tipi) => tipi switch
    {
        BelgeTipi.Cek => "Çek",
        BelgeTipi.Senet => "Senet",
        _ => tipi.ToString()
    };

    private static string DurumMetni(CekSenetDurum durum) => durum switch
    {
        CekSenetDurum.Portfoyde => "Portföyde",
        CekSenetDurum.Tahsilde => "Tahsilde",
        CekSenetDurum.Ciro => "Ciro",
        CekSenetDurum.Karsiliksiz => "Karşılıksız",
        CekSenetDurum.TahsilEdildi => "Tahsil Edildi",
        _ => durum.ToString()
    };
}
