using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class AlisSiparisiController : Controller
{
    private readonly IAlisSiparisiService _alisSiparisiService;
    private readonly ICariService _cariService;
    private readonly ISubeService _subeService;
    private readonly IMalzemeService _malzemeService;

    public AlisSiparisiController(
        IAlisSiparisiService alisSiparisiService, ICariService cariService, ISubeService subeService, IMalzemeService malzemeService)
    {
        _alisSiparisiService = alisSiparisiService;
        _cariService = cariService;
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

        var sutunAramalari = new string?[8];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _alisSiparisiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(s => new AlisSiparisiListItemViewModel
        {
            Id = s.Id,
            SiparisNo = s.SiparisNo,
            Tarih = s.Tarih,
            CariUnvan = s.Cari.Unvan,
            SubeAdi = s.Sube.SubeAdi,
            Durum = s.Durum.ToString(),
            DurumText = DurumMetni(s.Durum),
            TeslimDurumu = TeslimDurumuMetni(s, _alisSiparisiService.TeslimMiktarlariHesapla(s)),
            ToplamTutar = s.Kalemler.Sum(KalemToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Detay(int id)
    {
        var siparis = await _alisSiparisiService.GetByIdDetayAsync(id);
        if (siparis is null)
            return NotFound();

        var teslimMiktarlari = _alisSiparisiService.TeslimMiktarlariHesapla(siparis);

        var vm = new AlisSiparisiDetayViewModel
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            Tarih = siparis.Tarih,
            CariId = siparis.CariId,
            CariUnvan = siparis.Cari.Unvan,
            SubeAdi = siparis.Sube.SubeAdi,
            Aciklama = siparis.Aciklama,
            Durum = siparis.Durum,
            DurumText = DurumMetni(siparis.Durum),
            Kalemler = siparis.Kalemler.Select(k =>
            {
                var teslimAlinan = Math.Min(teslimMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar);
                return new AlisSiparisiKalemDetayViewModel
                {
                    MalzemeKodu = k.Malzeme.MalzemeKodu,
                    MalzemeAdi = k.Malzeme.MalzemeAdi,
                    Birim = k.Malzeme.Birim,
                    Miktar = k.Miktar,
                    BirimFiyat = k.BirimFiyat,
                    KdvOrani = k.KdvOrani,
                    Iskonto = k.Iskonto,
                    TeslimAlinanMiktar = teslimAlinan,
                    KalanMiktar = k.Miktar - teslimAlinan,
                    TeslimYuzdesi = k.Miktar == 0 ? 0 : Math.Round(teslimAlinan / k.Miktar * 100, 1)
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
            await _alisSiparisiService.OnaylaAsync(id);
            TempData["Basari"] = "Alış siparişi onaylandı.";
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
            await _alisSiparisiService.IptalEtAsync(id);
            TempData["Basari"] = "Alış siparişi iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new AlisSiparisiFormViewModel();
        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(AlisSiparisiFormViewModel vm)
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
            var siparis = new AlisSiparisi
            {
                Tarih = vm.Tarih,
                CariId = vm.CariId!.Value,
                SubeId = vm.SubeId!.Value,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new AlisSiparisiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();

            await _alisSiparisiService.CreateAsync(siparis, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Alış siparişi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task DoldurListeler(AlisSiparisiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.TedarikciListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Tedarikci or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var subeler = await _subeService.GetAllAsync();
        vm.SubeListesi = subeler.Where(s => !s.IsDeleted)
            .Select(s => new SelectListItem(s.SubeAdi, s.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static decimal KalemToplami(AlisSiparisiKalemi k)
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

    private static string TeslimDurumuMetni(AlisSiparisi siparis, Dictionary<int, decimal> teslimMiktarlari)
    {
        if (siparis.Durum == BelgeDurum.Iptal)
            return "-";

        var siparisToplami = siparis.Kalemler.Sum(k => k.Miktar);
        var teslimToplami = siparis.Kalemler.Sum(k => Math.Min(teslimMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar));

        if (teslimToplami <= 0)
            return "Teslim Edilmedi";
        if (teslimToplami < siparisToplami)
            return "Kısmi Teslim Alındı";
        return "Tamamen Teslim Alındı";
    }
}
