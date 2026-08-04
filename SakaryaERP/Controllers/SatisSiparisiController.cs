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

[Authorize(Roles = "Admin,Satis")]
public class SatisSiparisiController : Controller
{
    private readonly ISatisSiparisiService _satisSiparisiService;
    private readonly ISatisTeklifiService _satisTeklifiService;
    private readonly ICariService _cariService;
    private readonly IMalzemeService _malzemeService;
    private readonly ISatisFaturasiService _satisFaturasiService;

    public SatisSiparisiController(
        ISatisSiparisiService satisSiparisiService,
        ISatisTeklifiService satisTeklifiService,
        ICariService cariService,
        IMalzemeService malzemeService,
        ISatisFaturasiService satisFaturasiService)
    {
        _satisSiparisiService = satisSiparisiService;
        _satisTeklifiService = satisTeklifiService;
        _cariService = cariService;
        _malzemeService = malzemeService;
        _satisFaturasiService = satisFaturasiService;
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
            ToplamTutar = s.Kalemler.Sum(FinansHesaplama.SatirToplami)
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
            if (await _satisSiparisiService.AktifSiparisVarMiTeklifIcinAsync(teklifId.Value))
            {
                TempData["Hata"] = "Bu teklif için zaten bir sipariş oluşturulmuş.";
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

    private List<BelgeZinciriAdimi> BelgeZinciriOlustur(SatisSiparisi siparis)
    {
        var zincir = new List<BelgeZinciriAdimi>();
        if (siparis.SatisTeklifi is not null)
        {
            zincir.Add(new BelgeZinciriAdimi
            {
                Etiket = "Teklif",
                Metin = siparis.SatisTeklifi.TeklifNo,
                Href = Url.Action("Detay", "SatisTeklifi", new { id = siparis.SatisTeklifiId })
            });
        }
        zincir.Add(new BelgeZinciriAdimi { Etiket = "Sipariş", Metin = siparis.SiparisNo, Aktif = true });
        return zincir;
    }

    public async Task<IActionResult> Excel()
    {
        var (kayitlar, _, _) = await _satisSiparisiService.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[7], -1, "desc");

        string[] basliklar = ["Sipariş No", "Tarih", "Cari", "Teklif No", "Durum", "Sevk Durumu", "Toplam Tutar"];
        var satirlar = kayitlar.Select(s => new object?[]
        {
            s.SiparisNo, s.Tarih, s.Cari.Unvan, s.SatisTeklifi?.TeklifNo ?? "-", DurumMetni(s.Durum),
            SevkDurumuMetni(s, _satisSiparisiService.SevkMiktarlariHesapla(s)), s.Kalemler.Sum(FinansHesaplama.SatirToplami)
        });

        var dosya = ExcelYardimcisi.ListeOlustur("Satış Siparişleri", basliklar, satirlar);
        return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"satis-siparisi-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
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
                    baslikSutunu.Item().Text("Satış Siparişi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"Sipariş No: {vm.SiparisNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Müşteri: {vm.CariUnvan}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(vm.TeklifNo))
                        baslikSutunu.Item().Text($"Teklif No: {vm.TeklifNo}").FontSize(10);
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

        var dosyaAdi = $"satis-siparisi-{vm.SiparisNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<SatisSiparisiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var siparis = await _satisSiparisiService.GetByIdDetayAsync(id);
        if (siparis is null)
            return null;

        var sevkMiktarlari = _satisSiparisiService.SevkMiktarlariHesapla(siparis);

        return new SatisSiparisiDetayViewModel
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            Tarih = siparis.Tarih,
            CariId = siparis.CariId,
            CariUnvan = siparis.Cari.Unvan,
            TeklifNo = siparis.SatisTeklifi?.TeklifNo,
            Aciklama = siparis.Aciklama,
            Durum = siparis.Durum,
            DurumText = DurumMetni(siparis.Durum),
            Zincir = BelgeZinciriOlustur(siparis),
            FaturaOlusturulabilirMi = !await _satisFaturasiService.AktifFaturaVarMiSiparisIcinAsync(siparis.Id),
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
    }

    private static IContainer PdfBaslikHucresi(IContainer container) =>
        container.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer PdfIcerikHucresi(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
}
