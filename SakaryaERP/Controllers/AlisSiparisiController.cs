using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SakaryaERP.Helpers;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
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
            ToplamTutar = s.Kalemler.Sum(FinansHesaplama.SatirToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
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
                TeslimTarihi = vm.TeslimTarihi,
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

    public async Task<IActionResult> Excel()
    {
        var (kayitlar, _, _) = await _alisSiparisiService.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[8], -1, "desc");

        string[] basliklar = ["Sipariş No", "Tarih", "Cari", "Şube", "Durum", "Teslim Durumu", "Toplam Tutar"];
        var satirlar = kayitlar.Select(s => new object?[]
        {
            s.SiparisNo, s.Tarih, s.Cari.Unvan, s.Sube.SubeAdi, DurumMetni(s.Durum),
            TeslimDurumuMetni(s, _alisSiparisiService.TeslimMiktarlariHesapla(s)), s.Kalemler.Sum(FinansHesaplama.SatirToplami)
        });

        var dosya = ExcelYardimcisi.ListeOlustur("Alış Siparişleri", basliklar, satirlar);
        return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"alis-siparisi-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
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
                    baslikSutunu.Item().Text("Alış Siparişi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"Sipariş No: {vm.SiparisNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Tedarikçi: {vm.CariUnvan}").FontSize(10);
                    baslikSutunu.Item().Text($"Şube: {vm.SubeAdi}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(vm.Aciklama))
                        baslikSutunu.Item().Text($"Açıklama: {vm.Aciklama}").FontSize(10);
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

        var dosyaAdi = $"alis-siparisi-{vm.SiparisNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<AlisSiparisiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var siparis = await _alisSiparisiService.GetByIdDetayAsync(id);
        if (siparis is null)
            return null;

        var teslimMiktarlari = _alisSiparisiService.TeslimMiktarlariHesapla(siparis);

        return new AlisSiparisiDetayViewModel
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            Tarih = siparis.Tarih,
            TeslimTarihi = siparis.TeslimTarihi,
            CariId = siparis.CariId,
            CariUnvan = siparis.Cari.Unvan,
            SubeAdi = siparis.Sube.SubeAdi,
            Aciklama = siparis.Aciklama,
            Durum = siparis.Durum,
            DurumText = DurumMetni(siparis.Durum),
            Zincir = [new BelgeZinciriAdimi { Etiket = "Sipariş", Metin = siparis.SiparisNo, Aktif = true }],
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
    }

    private static IContainer PdfBaslikHucresi(IContainer container) =>
        container.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer PdfIcerikHucresi(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
}
