using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Satis")]
public class SevkIrsaliyesiController : Controller
{
    private readonly ISevkIrsaliyesiService _sevkIrsaliyesiService;
    private readonly ISatisSiparisiService _satisSiparisiService;
    private readonly ISubeService _subeService;
    private readonly IMalzemeService _malzemeService;

    public SevkIrsaliyesiController(
        ISevkIrsaliyesiService sevkIrsaliyesiService,
        ISatisSiparisiService satisSiparisiService,
        ISubeService subeService,
        IMalzemeService malzemeService)
    {
        _sevkIrsaliyesiService = sevkIrsaliyesiService;
        _satisSiparisiService = satisSiparisiService;
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

        var sutunAramalari = new string?[7];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _sevkIrsaliyesiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(i => new SevkIrsaliyesiListItemViewModel
        {
            Id = i.Id,
            IrsaliyeNo = i.IrsaliyeNo,
            Tarih = i.Tarih,
            CariUnvan = i.SatisSiparisi.Cari.Unvan,
            SiparisNo = i.SatisSiparisi.SiparisNo,
            SubeAdi = i.Sube.SubeAdi,
            Durum = i.Durum.ToString(),
            DurumText = DurumMetni(i.Durum)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int siparisId)
    {
        var siparis = await _satisSiparisiService.GetByIdDetayAsync(siparisId);
        if (siparis is null)
        {
            TempData["Hata"] = "Satış siparişi bulunamadı.";
            return RedirectToAction("Index", "SatisSiparisi");
        }
        if (siparis.Durum != BelgeDurum.Onaylandi)
        {
            TempData["Hata"] = "Sevk irsaliyesi sadece onaylanmış bir siparişten oluşturulabilir.";
            return RedirectToAction("Detay", "SatisSiparisi", new { id = siparisId });
        }

        var sevkMiktarlari = _satisSiparisiService.SevkMiktarlariHesapla(siparis);
        var kalanKalemler = siparis.Kalemler
            .Select(k => new { k.MalzemeId, Kalan = k.Miktar - Math.Min(sevkMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar) })
            .Where(k => k.Kalan > 0)
            .ToList();

        if (kalanKalemler.Count == 0)
        {
            TempData["Hata"] = "Bu siparişin tüm kalemleri zaten sevk edilmiş.";
            return RedirectToAction("Detay", "SatisSiparisi", new { id = siparisId });
        }

        var vm = new SevkIrsaliyesiFormViewModel
        {
            SatisSiparisiId = siparis.Id,
            SiparisNoGosterim = siparis.SiparisNo,
            CariUnvanGosterim = siparis.Cari.Unvan,
            Kalemler = kalanKalemler
                .Select(k => new SevkIrsaliyesiKalemFormViewModel { MalzemeId = k.MalzemeId, Miktar = k.Kalan })
                .ToList()
        };

        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(SevkIrsaliyesiFormViewModel vm)
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
            var irsaliye = new SevkIrsaliyesi
            {
                Tarih = vm.Tarih,
                SatisSiparisiId = vm.SatisSiparisiId,
                SubeId = vm.SubeId!.Value,
                SevkAdresi = vm.SevkAdresi,
                AracSofor = vm.AracSofor
            };
            var kalemler = vm.Kalemler.Select(k => new SevkIrsaliyesiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar
            }).ToList();

            await _sevkIrsaliyesiService.CreateAsync(irsaliye, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Sevk irsaliyesi kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var vm = await BuildDetayViewModel(id);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        try
        {
            await _sevkIrsaliyesiService.OnaylaAsync(id);
            TempData["Basari"] = "Sevk irsaliyesi onaylandı, malzeme bakiyeleri güncellendi.";
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
            await _sevkIrsaliyesiService.IptalEtAsync(id);
            TempData["Basari"] = "Sevk irsaliyesi iptal edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Pdf(int id)
    {
        var vm = await BuildDetayViewModel(id);
        if (vm is null)
            return NotFound();

        var belge = Document.Create(container =>
        {
            container.Page(sayfa =>
            {
                sayfa.Size(PageSizes.A4);
                sayfa.Margin(30);
                sayfa.DefaultTextStyle(x => x.FontSize(9));

                sayfa.Header().Column(baslikSutunu =>
                {
                    baslikSutunu.Item().Text("Sevk İrsaliyesi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"İrsaliye No: {vm.IrsaliyeNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Müşteri: {vm.CariUnvan}").FontSize(10);
                    baslikSutunu.Item().Text($"Sipariş No: {vm.SiparisNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Şube: {vm.SubeAdi}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(vm.SevkAdresi))
                        baslikSutunu.Item().Text($"Sevk Adresi: {vm.SevkAdresi}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(vm.AracSofor))
                        baslikSutunu.Item().Text($"Araç / Şoför: {vm.AracSofor}").FontSize(10);
                });

                sayfa.Content().Table(tablo =>
                {
                    tablo.ColumnsDefinition(sutunlar =>
                    {
                        sutunlar.RelativeColumn(3);
                        sutunlar.RelativeColumn(1);
                        sutunlar.RelativeColumn(1);
                    });

                    tablo.Header(baslik =>
                    {
                        string[] basliklar = ["Malzeme", "Birim", "Miktar"];
                        foreach (var b in basliklar)
                            baslik.Cell().Element(PdfBaslikHucresi).Text(b);
                    });

                    foreach (var k in vm.Kalemler)
                    {
                        tablo.Cell().Element(PdfIcerikHucresi).Text($"{k.MalzemeKodu} - {k.MalzemeAdi}");
                        tablo.Cell().Element(PdfIcerikHucresi).Text(k.Birim);
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(k.Miktar.ToString("N2"));
                    }
                });
            });
        });

        var dosyaAdi = $"sevk-irsaliyesi-{vm.IrsaliyeNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<SevkIrsaliyesiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var irsaliye = await _sevkIrsaliyesiService.GetByIdDetayAsync(id);
        if (irsaliye is null)
            return null;

        return new SevkIrsaliyesiDetayViewModel
        {
            Id = irsaliye.Id,
            IrsaliyeNo = irsaliye.IrsaliyeNo,
            Tarih = irsaliye.Tarih,
            CariUnvan = irsaliye.SatisSiparisi.Cari.Unvan,
            SiparisNo = irsaliye.SatisSiparisi.SiparisNo,
            SubeAdi = irsaliye.Sube.SubeAdi,
            SevkAdresi = irsaliye.SevkAdresi,
            AracSofor = irsaliye.AracSofor,
            Durum = irsaliye.Durum,
            DurumText = DurumMetni(irsaliye.Durum),
            Zincir =
            [
                new BelgeZinciriAdimi
                {
                    Etiket = "Sipariş",
                    Metin = irsaliye.SatisSiparisi.SiparisNo,
                    Href = Url.Action("Detay", "SatisSiparisi", new { id = irsaliye.SatisSiparisiId })
                },
                new BelgeZinciriAdimi { Etiket = "Sevk İrsaliyesi", Metin = irsaliye.IrsaliyeNo, Aktif = true }
            ],
            Kalemler = irsaliye.Kalemler.Select(k => new SevkIrsaliyesiKalemDetayViewModel
            {
                MalzemeKodu = k.Malzeme.MalzemeKodu,
                MalzemeAdi = k.Malzeme.MalzemeAdi,
                Birim = k.Malzeme.Birim,
                Miktar = k.Miktar
            }).ToList()
        };
    }

    private async Task DoldurListeler(SevkIrsaliyesiFormViewModel vm)
    {
        var subeler = await _subeService.GetAllAsync();
        vm.SubeListesi = subeler.Where(s => !s.IsDeleted)
            .Select(s => new SelectListItem(s.SubeAdi, s.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static IContainer PdfBaslikHucresi(IContainer container) =>
        container.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer PdfIcerikHucresi(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);

    private static string DurumMetni(BelgeDurum durum) => durum switch
    {
        BelgeDurum.Beklemede => "Beklemede",
        BelgeDurum.Onaylandi => "Onaylandı",
        BelgeDurum.Iptal => "İptal",
        _ => durum.ToString()
    };
}
