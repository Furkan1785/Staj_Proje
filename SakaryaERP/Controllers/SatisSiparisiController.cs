using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Satis")]
public class SatisSiparisiController : Controller
{
    private readonly ISatisSiparisiService _satisSiparisiService;
    private readonly ISatisTeklifiService _satisTeklifiService;
    private readonly ICariService _cariService;
    private readonly IMalzemeService _malzemeService;

    public SatisSiparisiController(
        ISatisSiparisiService satisSiparisiService,
        ISatisTeklifiService satisTeklifiService,
        ICariService cariService,
        IMalzemeService malzemeService)
    {
        _satisSiparisiService = satisSiparisiService;
        _satisTeklifiService = satisTeklifiService;
        _cariService = cariService;
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

        var sutunAramalari = new string?[7];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _satisSiparisiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(s => new SatisSiparisiListItemViewModel
        {
            Id = s.Id,
            SiparisNo = s.SiparisNo,
            Tarih = s.Tarih,
            CariUnvan = s.Cari.Unvan,
            TeklifNo = s.SatisTeklifi?.TeklifNo ?? "-",
            Durum = s.Durum.ToString(),
            DurumText = DurumMetni(s.Durum),
            SevkDurumu = SevkDurumuMetni(s, _satisSiparisiService.SevkMiktarlariHesapla(s)),
            ToplamTutar = s.Kalemler.Sum(KalemToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int? teklifId)
    {
        var vm = new SatisSiparisiFormViewModel();

        if (teklifId is not null)
        {
            var teklif = await _satisTeklifiService.GetByIdDetayAsync(teklifId.Value);
            if (teklif is null)
            {
                TempData["Hata"] = "Satış teklifi bulunamadı.";
                return RedirectToAction("Index", "SatisTeklifi");
            }
            if (teklif.Durum != BelgeDurum.Onaylandi)
            {
                TempData["Hata"] = "Sipariş sadece onaylanmış bir teklifden oluşturulabilir.";
                return RedirectToAction("Detay", "SatisTeklifi", new { id = teklifId });
            }

            vm.CariId = teklif.CariId;
            vm.SatisTeklifiId = teklif.Id;
            vm.TeklifNoGosterim = teklif.TeklifNo;
            vm.Aciklama = teklif.Aciklama;
            vm.Kalemler = teklif.Kalemler.Select(k => new SatisSiparisiKalemFormViewModel
            {
                MalzemeId = k.MalzemeId,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();
        }

        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(SatisSiparisiFormViewModel vm)
    {
        // Boş bırakılmış (malzeme seçilmemiş) satırları yok say.
        vm.Kalemler = vm.Kalemler.Where(k => k.MalzemeId is not null).ToList();

        if (!ModelState.IsValid)
        {
            await DoldurListeler(vm);
            return View(vm);
        }

        try
        {
            var siparis = new SatisSiparisi
            {
                Tarih = vm.Tarih,
                CariId = vm.CariId!.Value,
                SatisTeklifiId = vm.SatisTeklifiId,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new SatisSiparisiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();

            await _satisSiparisiService.CreateAsync(siparis, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Satış siparişi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var siparis = await _satisSiparisiService.GetByIdDetayAsync(id);
        if (siparis is null)
            return NotFound();

        var sevkMiktarlari = _satisSiparisiService.SevkMiktarlariHesapla(siparis);

        var vm = new SatisSiparisiDetayViewModel
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            Tarih = siparis.Tarih,
            CariUnvan = siparis.Cari.Unvan,
            TeklifNo = siparis.SatisTeklifi?.TeklifNo,
            Aciklama = siparis.Aciklama,
            Durum = siparis.Durum,
            DurumText = DurumMetni(siparis.Durum),
            Kalemler = siparis.Kalemler.Select(k =>
            {
                var sevkEdilen = Math.Min(sevkMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar);
                return new SatisSiparisiKalemDetayViewModel
                {
                    MalzemeKodu = k.Malzeme.MalzemeKodu,
                    MalzemeAdi = k.Malzeme.MalzemeAdi,
                    Birim = k.Malzeme.Birim,
                    Miktar = k.Miktar,
                    BirimFiyat = k.BirimFiyat,
                    KdvOrani = k.KdvOrani,
                    Iskonto = k.Iskonto,
                    SevkEdilenMiktar = sevkEdilen,
                    KalanMiktar = k.Miktar - sevkEdilen,
                    SevkYuzdesi = k.Miktar == 0 ? 0 : Math.Round(sevkEdilen / k.Miktar * 100, 1)
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        try
        {
            await _satisSiparisiService.OnaylaAsync(id);
            TempData["Basari"] = "Satış siparişi onaylandı.";
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
            await _satisSiparisiService.IptalEtAsync(id);
            TempData["Basari"] = "Satış siparişi iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task DoldurListeler(SatisSiparisiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.MusteriListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Musteri or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static decimal KalemToplami(SatisSiparisiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
    }

    private static string DurumMetni(BelgeDurum durum) => durum switch
    {
        BelgeDurum.Beklemede => "Beklemede",
        BelgeDurum.Onaylandi => "Onaylandı",
        BelgeDurum.Iptal => "İptal",
        _ => durum.ToString()
    };

    private static string SevkDurumuMetni(SatisSiparisi siparis, Dictionary<int, decimal> sevkMiktarlari)
    {
        if (siparis.Durum == BelgeDurum.Iptal)
            return "-";

        var siparisToplami = siparis.Kalemler.Sum(k => k.Miktar);
        var sevkToplami = siparis.Kalemler.Sum(k => Math.Min(sevkMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar));

        if (sevkToplami <= 0)
            return "Sevk Edilmedi";
        if (sevkToplami < siparisToplami)
            return "Kısmi Sevkiyat";
        return "Tamamen Sevk Edildi";
    }
}
