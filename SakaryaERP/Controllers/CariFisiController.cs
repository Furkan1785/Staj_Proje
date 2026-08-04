using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class CariFisiController : Controller
{
    private readonly ICariFisiService _cariFisiService;
    private readonly ICariService _cariService;
    private readonly IBankaHesabiService _bankaHesabiService;
    private readonly IKasaHesabiService _kasaHesabiService;

    public CariFisiController(
        ICariFisiService cariFisiService,
        ICariService cariService,
        IBankaHesabiService bankaHesabiService,
        IKasaHesabiService kasaHesabiService)
    {
        _cariFisiService = cariFisiService;
        _cariService = cariService;
        _bankaHesabiService = bankaHesabiService;
        _kasaHesabiService = kasaHesabiService;
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
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "1");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "desc";

        var sutunAramalari = new string?[8];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _cariFisiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(f => new CariFisiListItemViewModel
        {
            Id = f.Id,
            FisNo = f.FisNo,
            Tarih = f.Tarih,
            CariUnvan = f.Cari.Unvan,
            FisTipiText = FisTipiMetni(f.FisTipi),
            Tutar = f.Tutar,
            OdemeYontemiText = OdemeYontemiMetni(f.OdemeYontemi),
            HesapAdi = f.BankaHesabi?.HesapAdi ?? f.KasaHesabi?.KasaAdi,
            Aciklama = f.Aciklama
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new CariFisiFormViewModel();
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(CariFisiFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            await _cariFisiService.CreateAsync(new CariFisi
            {
                CariId = vm.CariId,
                Tarih = vm.Tarih,
                FisTipi = vm.FisTipi,
                Tutar = vm.Tutar,
                OdemeYontemi = vm.OdemeYontemi,
                BankaHesabiId = vm.BankaHesabiId,
                KasaHesabiId = vm.KasaHesabiId,
                Aciklama = vm.Aciklama
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Cari fişi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var fis = await _cariFisiService.GetByIdDetayAsync(id);
        if (fis is null)
            return NotFound();

        var vm = new CariFisiDetayViewModel
        {
            Id = fis.Id,
            FisNo = fis.FisNo,
            Tarih = fis.Tarih,
            CariId = fis.CariId,
            CariUnvan = fis.Cari.Unvan,
            FisTipi = fis.FisTipi,
            FisTipiText = FisTipiMetni(fis.FisTipi),
            Tutar = fis.Tutar,
            OdemeYontemiText = OdemeYontemiMetni(fis.OdemeYontemi),
            HesapAdi = fis.BankaHesabi?.HesapAdi ?? fis.KasaHesabi?.KasaAdi,
            Aciklama = fis.Aciklama,
            IptalEdildi = fis.IsDeleted,
            OtomatikOlusturuldu = fis.OtomatikOlusturuldu
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id)
    {
        try
        {
            await _cariFisiService.IptalEtAsync(id);
            TempData["Basari"] = "Cari fişi iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(Detay), new { id });
    }

    private async Task DoldurListeler(CariFisiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.CariListesi = cariler.Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var bankaHesaplari = await _bankaHesabiService.GetAllAsync();
        vm.BankaHesabiListesi = bankaHesaplari.Where(b => !b.IsDeleted)
            .Select(b => new SelectListItem($"{b.HesapAdi} ({b.BankaAdi})", b.Id.ToString()));

        var kasaHesaplari = await _kasaHesabiService.GetAllAsync();
        vm.KasaHesabiListesi = kasaHesaplari.Where(k => !k.IsDeleted)
            .Select(k => new SelectListItem(k.KasaAdi, k.Id.ToString()));
    }

    private static string FisTipiMetni(FisTipi tipi) => tipi switch
    {
        FisTipi.Borc => "Borç",
        FisTipi.Alacak => "Alacak",
        FisTipi.Mahsup => "Mahsup",
        _ => tipi.ToString()
    };

    private static string OdemeYontemiMetni(OdemeYontemi yontem) => yontem switch
    {
        OdemeYontemi.Nakit => "Nakit",
        OdemeYontemi.Havale => "Havale",
        OdemeYontemi.KrediKarti => "Kredi Kartı",
        _ => yontem.ToString()
    };
}
