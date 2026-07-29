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
public class SatisTeklifiController : Controller
{
    private readonly ISatisTeklifiService _satisTeklifiService;
    private readonly IMusteriTalebiService _musteriTalebiService;
    private readonly ICariService _cariService;
    private readonly IMalzemeService _malzemeService;

    public SatisTeklifiController(
        ISatisTeklifiService satisTeklifiService,
        IMusteriTalebiService musteriTalebiService,
        ICariService cariService,
        IMalzemeService malzemeService)
    {
        _satisTeklifiService = satisTeklifiService;
        _musteriTalebiService = musteriTalebiService;
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

        var sutunAramalari = new string?[8];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _satisTeklifiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(t => new SatisTeklifiListItemViewModel
        {
            Id = t.Id,
            TeklifNo = t.TeklifNo,
            Tarih = t.Tarih,
            GecerlilikTarihi = t.GecerlilikTarihi,
            CariUnvan = t.Cari.Unvan,
            TalepNo = t.MusteriTalebi?.TalepNo ?? "-",
            Durum = t.Durum.ToString(),
            DurumText = DurumMetni(t.Durum),
            ToplamTutar = t.Kalemler.Sum(KalemToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int? talepId)
    {
        var vm = new SatisTeklifiFormViewModel();

        if (talepId is not null)
        {
            var talep = await _musteriTalebiService.GetByIdAsync(talepId.Value);
            if (talep is null)
            {
                TempData["Hata"] = "Müşteri talebi bulunamadı.";
                return RedirectToAction("Index", "MusteriTalebi");
            }
            if (talep.Durum is not (TalepDurum.Yeni or TalepDurum.Isleniyor))
            {
                TempData["Hata"] = "Sadece yeni veya işlemedeki bir talepten teklif oluşturulabilir.";
                return RedirectToAction("Index", "MusteriTalebi");
            }

            vm.CariId = talep.CariId;
            vm.MusteriTalebiId = talep.Id;
            vm.TalepNoGosterim = talep.TalepNo;
            vm.Aciklama = talep.Icerik;
        }

        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(SatisTeklifiFormViewModel vm)
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
            var teklif = new SatisTeklifi
            {
                Tarih = vm.Tarih,
                GecerlilikTarihi = vm.GecerlilikTarihi,
                CariId = vm.CariId!.Value,
                MusteriTalebiId = vm.MusteriTalebiId,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new SatisTeklifiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();

            await _satisTeklifiService.CreateAsync(teklif, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Satış teklifi kaydedildi.";
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
            await _satisTeklifiService.OnaylaAsync(id);
            TempData["Basari"] = "Teklif onaylandı.";
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
            await _satisTeklifiService.IptalEtAsync(id);
            TempData["Basari"] = "Teklif iptal edildi.";
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
                    baslikSutunu.Item().Text("Satış Teklifi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"Teklif No: {vm.TeklifNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    if (vm.GecerlilikTarihi is not null)
                        baslikSutunu.Item().Text($"Geçerlilik Tarihi: {vm.GecerlilikTarihi:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Müşteri: {vm.CariUnvan}").FontSize(10);
                });

                sayfa.Content().Table(tablo =>
                {
                    tablo.ColumnsDefinition(sutunlar =>
                    {
                        sutunlar.RelativeColumn(3);
                        sutunlar.RelativeColumn(1);
                        sutunlar.RelativeColumn(1);
                        sutunlar.RelativeColumn(1);
                        sutunlar.RelativeColumn(1);
                        sutunlar.RelativeColumn(1);
                    });

                    tablo.Header(baslik =>
                    {
                        string[] basliklar = ["Malzeme", "Miktar", "Birim Fiyat", "İskonto %", "KDV %", "Tutar"];
                        foreach (var b in basliklar)
                            baslik.Cell().Element(PdfBaslikHucresi).Text(b);
                    });

                    foreach (var k in vm.Kalemler)
                    {
                        tablo.Cell().Element(PdfIcerikHucresi).Text($"{k.MalzemeKodu} - {k.MalzemeAdi}");
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text($"{k.Miktar:N2} {k.Birim}");
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(k.BirimFiyat.ToString("N2"));
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(k.Iskonto.ToString("N2"));
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(k.KdvOrani.ToString("N2"));
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(k.SatirToplami.ToString("N2"));
                    }
                });

                sayfa.Footer().AlignRight().Text($"Genel Toplam: {vm.ToplamTutar:N2}").FontSize(11).Bold();
            });
        });

        var dosyaAdi = $"satis-teklifi-{vm.TeklifNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<SatisTeklifiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var teklif = await _satisTeklifiService.GetByIdDetayAsync(id);
        if (teklif is null)
            return null;

        return new SatisTeklifiDetayViewModel
        {
            Id = teklif.Id,
            TeklifNo = teklif.TeklifNo,
            Tarih = teklif.Tarih,
            GecerlilikTarihi = teklif.GecerlilikTarihi,
            CariUnvan = teklif.Cari.Unvan,
            TalepNo = teklif.MusteriTalebi?.TalepNo,
            Aciklama = teklif.Aciklama,
            Durum = teklif.Durum,
            DurumText = DurumMetni(teklif.Durum),
            Kalemler = teklif.Kalemler.Select(k => new SatisTeklifiKalemDetayViewModel
            {
                MalzemeKodu = k.Malzeme.MalzemeKodu,
                MalzemeAdi = k.Malzeme.MalzemeAdi,
                Birim = k.Malzeme.Birim,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList()
        };
    }

    private async Task DoldurListeler(SatisTeklifiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.MusteriListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Musteri or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static decimal KalemToplami(SatisTeklifiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
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
