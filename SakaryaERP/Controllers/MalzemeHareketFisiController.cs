using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Helpers;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe,Satis")]
public class MalzemeHareketFisiController : Controller
{
    private readonly IMalzemeHareketFisiService _malzemeHareketFisiService;
    private readonly ISubeService _subeService;
    private readonly IMalzemeService _malzemeService;

    public MalzemeHareketFisiController(
        IMalzemeHareketFisiService malzemeHareketFisiService, ISubeService subeService, IMalzemeService malzemeService)
    {
        _malzemeHareketFisiService = malzemeHareketFisiService;
        _subeService = subeService;
        _malzemeService = malzemeService;
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

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _malzemeHareketFisiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(f => new MalzemeHareketFisiListItemViewModel
        {
            Id = f.Id,
            FisNo = f.FisNo,
            Tarih = f.Tarih,
            HareketTipiText = HareketTipiMetni(f.HareketTipi),
            SubeAdi = f.Sube.SubeAdi,
            KalemSayisi = f.Kalemler.Count,
            Durum = f.Durum.ToString(),
            DurumText = DurumMetni(f.Durum)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new MalzemeHareketFisiFormViewModel();
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(MalzemeHareketFisiFormViewModel vm)
    {
        // Boş bırakılmış (malzeme seçilmemiş) satırları yok say — kullanıcı fazladan
        // satır ekleyip kullanmadan bırakmış olabilir, bu bir hata değil.
        vm.Kalemler = vm.Kalemler.Where(k => k.MalzemeId is not null).ToList();

        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            var fis = new MalzemeHareketFisi
            {
                Tarih = vm.Tarih,
                HareketTipi = vm.HareketTipi,
                SubeId = vm.SubeId!.Value
            };
            var kalemler = vm.Kalemler.Select(k => new MalzemeHareketFisiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                Aciklama = k.Aciklama
            }).ToList();

            await _malzemeHareketFisiService.CreateAsync(fis, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Malzeme hareket fişi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var fis = await _malzemeHareketFisiService.GetByIdDetayAsync(id);
        if (fis is null) return NotFound();

        return View(new MalzemeHareketFisiDetayViewModel
        {
            Id = fis.Id,
            FisNo = fis.FisNo,
            Tarih = fis.Tarih,
            HareketTipi = fis.HareketTipi,
            HareketTipiText = HareketTipiMetni(fis.HareketTipi),
            SubeAdi = fis.Sube.SubeAdi,
            Durum = fis.Durum,
            DurumText = DurumMetni(fis.Durum),
            Kalemler = fis.Kalemler.Select(k => new MalzemeHareketFisiKalemDetayViewModel
            {
                MalzemeKodu = k.Malzeme.MalzemeKodu,
                MalzemeAdi = k.Malzeme.MalzemeAdi,
                Birim = k.Malzeme.Birim,
                Miktar = k.Miktar,
                Aciklama = k.Aciklama
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id, bool returnToDetay = false)
    {
        try
        {
            await _malzemeHareketFisiService.OnaylaAsync(id);
            TempData["Basari"] = "Fiş onaylandı, malzeme bakiyeleri güncellendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id, bool returnToDetay = false)
    {
        try
        {
            await _malzemeHareketFisiService.IptalEtAsync(id);
            TempData["Basari"] = "Fiş iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return returnToDetay ? RedirectToAction(nameof(Detay), new { id }) : RedirectToAction(nameof(Index));
    }

    private async Task DoldurListeler(MalzemeHareketFisiFormViewModel vm)
    {
        var subeler = await _subeService.GetAllAsync();
        vm.SubeListesi = subeler.Where(s => !s.IsDeleted)
            .Select(s => new SelectListItem(s.SubeAdi, s.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim, barkod = m.Barkod });
    }

    private static string HareketTipiMetni(HareketTipi tip) => tip switch
    {
        HareketTipi.Giris => "Giriş",
        HareketTipi.Cikis => "Çıkış",
        HareketTipi.Transfer => "Transfer",
        HareketTipi.Fire => "Fire",
        _ => tip.ToString()
    };

    private static string DurumMetni(BelgeDurum durum) => durum switch
    {
        BelgeDurum.Beklemede => "Beklemede",
        BelgeDurum.Onaylandi => "Onaylandı",
        BelgeDurum.Iptal => "İptal",
        _ => durum.ToString()
    };

    public async Task<IActionResult> Excel()
    {
        var (kayitlar, _, _) = await _malzemeHareketFisiService.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[6], -1, "desc");

        string[] basliklar = ["Fiş No", "Tarih", "Hareket Tipi", "Şube", "Kalem Sayısı", "Durum"];
        var satirlar = kayitlar.Select(f => new object?[]
        {
            f.FisNo, f.Tarih, HareketTipiMetni(f.HareketTipi), f.Sube.SubeAdi, f.Kalemler.Count, DurumMetni(f.Durum)
        });

        var dosya = ExcelYardimcisi.ListeOlustur("Malzeme Hareket Fişleri", basliklar, satirlar);
        return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"malzeme-hareket-fisi-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
    }
}
