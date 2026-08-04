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

[Authorize(Roles = "Admin,Muhasebe")]
public class AlisFaturasiController : Controller
{
    private readonly IAlisFaturasiService _alisFaturasiService;
    private readonly IAlisIrsaliyesiService _alisIrsaliyesiService;
    private readonly IAlisSiparisiService _alisSiparisiService;
    private readonly ICariService _cariService;
    private readonly IMalzemeService _malzemeService;

    public AlisFaturasiController(
        IAlisFaturasiService alisFaturasiService,
        IAlisIrsaliyesiService alisIrsaliyesiService,
        IAlisSiparisiService alisSiparisiService,
        ICariService cariService,
        IMalzemeService malzemeService)
    {
        _alisFaturasiService = alisFaturasiService;
        _alisIrsaliyesiService = alisIrsaliyesiService;
        _alisSiparisiService = alisSiparisiService;
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

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _alisFaturasiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(f => new AlisFaturasiListItemViewModel
        {
            Id = f.Id,
            FaturaNo = f.FaturaNo,
            Tarih = f.Tarih,
            CariUnvan = f.Cari.Unvan,
            IrsaliyeNo = f.AlisIrsaliyesi?.IrsaliyeNo ?? "-",
            Durum = f.Durum.ToString(),
            DurumText = DurumMetni(f.Durum),
            ToplamTutar = f.Kalemler.Sum(KalemToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int? irsaliyeId)
    {
        var vm = new AlisFaturasiFormViewModel();

        if (irsaliyeId is not null)
        {
            var irsaliye = await _alisIrsaliyesiService.GetByIdDetayAsync(irsaliyeId.Value);
            if (irsaliye is null)
            {
                TempData["Hata"] = "Alış irsaliyesi bulunamadı.";
                return RedirectToAction("Index", "AlisIrsaliyesi");
            }
            if (irsaliye.Durum != BelgeDurum.Onaylandi)
            {
                TempData["Hata"] = "Fatura sadece onaylanmış bir irsaliyeden oluşturulabilir.";
                return RedirectToAction("Detay", "AlisIrsaliyesi", new { id = irsaliyeId });
            }
            if (await _alisFaturasiService.AktifFaturaVarMiAsync(irsaliye.Id))
            {
                TempData["Hata"] = "Bu irsaliye için zaten bir fatura oluşturulmuş.";
                return RedirectToAction("Detay", "AlisIrsaliyesi", new { id = irsaliyeId });
            }

            // Fiyat/KDV/iskonto bilgisi irsaliyede tutulmuyor; bağlı olduğu siparişteki
            // aynı malzemenin kalemi varsa oradan devralınır, yoksa malzeme kartındaki
            // güncel alış fiyatı varsayılan olarak kullanılır.
            var siparisFiyatlari = new Dictionary<int, (decimal BirimFiyat, decimal KdvOrani, decimal Iskonto)>();
            if (irsaliye.AlisSiparisiId is not null)
            {
                var siparis = await _alisSiparisiService.GetByIdDetayAsync(irsaliye.AlisSiparisiId.Value);
                if (siparis is not null)
                {
                    foreach (var k in siparis.Kalemler)
                        siparisFiyatlari[k.MalzemeId] = (k.BirimFiyat, k.KdvOrani, k.Iskonto);
                }
            }

            vm.CariId = irsaliye.CariId;
            vm.AlisIrsaliyesiId = irsaliye.Id;
            vm.IrsaliyeNoGosterim = irsaliye.IrsaliyeNo;
            vm.Kalemler = irsaliye.Kalemler.Select(k =>
            {
                var fiyatBulundu = siparisFiyatlari.TryGetValue(k.MalzemeId, out var fiyat);
                return new AlisFaturasiKalemFormViewModel
                {
                    MalzemeId = k.MalzemeId,
                    Miktar = k.Miktar,
                    BirimFiyat = fiyatBulundu ? fiyat.BirimFiyat : k.Malzeme.AlisFiyati,
                    KdvOrani = fiyatBulundu ? fiyat.KdvOrani : k.Malzeme.KdvOrani,
                    Iskonto = fiyatBulundu ? fiyat.Iskonto : 0
                };
            }).ToList();
        }

        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(AlisFaturasiFormViewModel vm)
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
            var fatura = new AlisFaturasi
            {
                Tarih = vm.Tarih,
                CariId = vm.CariId!.Value,
                AlisIrsaliyesiId = vm.AlisIrsaliyesiId,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new AlisFaturasiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();

            await _alisFaturasiService.CreateAsync(fatura, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Alış faturası kaydedildi.";
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
            await _alisFaturasiService.OnaylaAsync(id);
            TempData["Basari"] = "Fatura onaylandı, cari hareketi oluşturuldu.";
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
            await _alisFaturasiService.IptalEtAsync(id);
            TempData["Basari"] = "Fatura iptal edildi.";
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
                    baslikSutunu.Item().Text("Alış Faturası").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"Fatura No: {vm.FaturaNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Tedarikçi: {vm.CariUnvan}").FontSize(10);
                    if (vm.IrsaliyeNo is not null)
                        baslikSutunu.Item().Text($"İrsaliye No: {vm.IrsaliyeNo}").FontSize(10);
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

        var dosyaAdi = $"alis-faturasi-{vm.FaturaNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<AlisFaturasiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var fatura = await _alisFaturasiService.GetByIdDetayAsync(id);
        if (fatura is null)
            return null;

        return new AlisFaturasiDetayViewModel
        {
            Id = fatura.Id,
            FaturaNo = fatura.FaturaNo,
            Tarih = fatura.Tarih,
            CariId = fatura.CariId,
            CariUnvan = fatura.Cari.Unvan,
            SiparisNo = fatura.AlisSiparisi?.SiparisNo,
            IrsaliyeNo = fatura.AlisIrsaliyesi?.IrsaliyeNo,
            Aciklama = fatura.Aciklama,
            Durum = fatura.Durum,
            DurumText = DurumMetni(fatura.Durum),
            Zincir = BelgeZinciriOlustur(fatura),
            Kalemler = fatura.Kalemler.Select(k => new AlisFaturasiKalemDetayViewModel
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

    private async Task DoldurListeler(AlisFaturasiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.TedarikciListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Tedarikci or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static decimal KalemToplami(AlisFaturasiKalemi k)
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

    private List<BelgeZinciriAdimi> BelgeZinciriOlustur(AlisFaturasi fatura)
    {
        var zincir = new List<BelgeZinciriAdimi>();
        if (fatura.AlisSiparisi is not null)
        {
            zincir.Add(new BelgeZinciriAdimi
            {
                Etiket = "Sipariş",
                Metin = fatura.AlisSiparisi.SiparisNo,
                Href = Url.Action("Detay", "AlisSiparisi", new { id = fatura.AlisSiparisiId })
            });
        }
        if (fatura.AlisIrsaliyesi is not null)
        {
            zincir.Add(new BelgeZinciriAdimi
            {
                Etiket = "İrsaliye",
                Metin = fatura.AlisIrsaliyesi.IrsaliyeNo,
                Href = Url.Action("Detay", "AlisIrsaliyesi", new { id = fatura.AlisIrsaliyesiId })
            });
        }
        zincir.Add(new BelgeZinciriAdimi { Etiket = "Fatura", Metin = fatura.FaturaNo, Aktif = true });
        return zincir;
    }
}
