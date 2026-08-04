using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Satis")]
public class MusteriTalebiController : Controller
{
    private readonly IMusteriTalebiService _musteriTalebiService;
    private readonly ICariService _cariService;

    public MusteriTalebiController(IMusteriTalebiService musteriTalebiService, ICariService cariService)
    {
        _musteriTalebiService = musteriTalebiService;
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
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "1");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "desc";

        var sutunAramalari = new string?[6];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _musteriTalebiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(t => new MusteriTalebiListItemViewModel
        {
            Id = t.Id,
            TalepNo = t.TalepNo,
            Tarih = t.Tarih,
            CariUnvan = t.Cari.Unvan,
            Icerik = t.Icerik ?? "",
            Durum = t.Durum.ToString(),
            DurumText = DurumMetni(t.Durum)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new MusteriTalebiFormViewModel();
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(MusteriTalebiFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            await _musteriTalebiService.CreateAsync(new MusteriTalebi
            {
                Tarih = vm.Tarih,
                CariId = vm.CariId!.Value,
                Icerik = vm.Icerik
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Müşteri talebi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var talep = await _musteriTalebiService.GetByIdDetayAsync(id);
        if (talep is null)
            return NotFound();

        var vm = new MusteriTalebiDetayViewModel
        {
            Id = talep.Id,
            TalepNo = talep.TalepNo,
            Tarih = talep.Tarih,
            CariId = talep.CariId,
            CariUnvan = talep.Cari.Unvan,
            Icerik = talep.Icerik,
            Durum = talep.Durum,
            DurumText = DurumMetni(talep.Durum),
            Teklifler = talep.SatisTeklifleri.Select(t => new MusteriTalebiTeklifOzetViewModel
            {
                Id = t.Id,
                TeklifNo = t.TeklifNo,
                Tarih = t.Tarih,
                DurumText = BelgeDurumMetni(t.Durum)
            }).OrderByDescending(t => t.Tarih).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IslemeAl(int id)
    {
        try
        {
            await _musteriTalebiService.IslemeAlAsync(id);
            TempData["Basari"] = "Talep işleme alındı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id)
    {
        try
        {
            await _musteriTalebiService.IptalEtAsync(id);
            TempData["Basari"] = "Talep iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task DoldurListeler(MusteriTalebiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.MusteriListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Musteri or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));
    }

    private static string DurumMetni(TalepDurum durum) => durum switch
    {
        TalepDurum.Yeni => "Yeni",
        TalepDurum.Isleniyor => "İşleniyor",
        TalepDurum.Tamamlandi => "Tamamlandı",
        TalepDurum.Iptal => "İptal",
        _ => durum.ToString()
    };

    private static string BelgeDurumMetni(BelgeDurum durum) => durum switch
    {
        BelgeDurum.Beklemede => "Beklemede",
        BelgeDurum.Onaylandi => "Onaylandı",
        BelgeDurum.Iptal => "İptal",
        _ => durum.ToString()
    };

    public async Task<IActionResult> Excel()
    {
        var (kayitlar, _, _) = await _musteriTalebiService.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[6], -1, "desc");

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Müşteri Talepleri");

        string[] basliklar = ["Talep No", "Tarih", "Cari", "İçerik", "Durum"];
        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = basliklar[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var satirNo = 2;
        foreach (var t in kayitlar)
        {
            sayfa.Cell(satirNo, 1).Value = t.TalepNo;
            sayfa.Cell(satirNo, 2).Value = t.Tarih;
            sayfa.Cell(satirNo, 2).Style.DateFormat.Format = "dd.MM.yyyy";
            sayfa.Cell(satirNo, 3).Value = t.Cari.Unvan;
            sayfa.Cell(satirNo, 4).Value = t.Icerik;
            sayfa.Cell(satirNo, 5).Value = DurumMetni(t.Durum);
            satirNo++;
        }
        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"musteri-talebi-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
    }
}
