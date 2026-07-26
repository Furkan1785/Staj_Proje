using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class SatisFaturasiController : Controller
{
    private readonly ISatisFaturasiService _satisFaturasiService;
    private readonly ISevkIrsaliyesiService _sevkIrsaliyesiService;
    private readonly ISatisSiparisiService _satisSiparisiService;
    private readonly ICariService _cariService;
    private readonly IMalzemeService _malzemeService;

    public SatisFaturasiController(
        ISatisFaturasiService satisFaturasiService,
        ISevkIrsaliyesiService sevkIrsaliyesiService,
        ISatisSiparisiService satisSiparisiService,
        ICariService cariService,
        IMalzemeService malzemeService)
    {
        _satisFaturasiService = satisFaturasiService;
        _sevkIrsaliyesiService = sevkIrsaliyesiService;
        _satisSiparisiService = satisSiparisiService;
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

        var sutunAramalari = new string?[9];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _satisFaturasiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(f => new SatisFaturasiListItemViewModel
        {
            Id = f.Id,
            FaturaNo = f.FaturaNo,
            Tarih = f.Tarih,
            VadeTarihi = f.VadeTarihi,
            CariUnvan = f.Cari.Unvan,
            SiparisNo = f.SatisSiparisi?.SiparisNo ?? "-",
            IrsaliyeNo = f.SevkIrsaliyesi?.IrsaliyeNo ?? "-",
            Durum = f.Durum.ToString(),
            DurumText = DurumMetni(f.Durum),
            ToplamTutar = f.Kalemler.Sum(KalemToplami)
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int? irsaliyeId, int? siparisId)
    {
        var vm = new SatisFaturasiFormViewModel();

        if (irsaliyeId is not null)
        {
            var irsaliye = await _sevkIrsaliyesiService.GetByIdDetayAsync(irsaliyeId.Value);
            if (irsaliye is null)
            {
                TempData["Hata"] = "Sevk irsaliyesi bulunamadı.";
                return RedirectToAction("Index", "SevkIrsaliyesi");
            }
            if (irsaliye.Durum != BelgeDurum.Onaylandi)
            {
                TempData["Hata"] = "Fatura sadece onaylanmış bir irsaliyeden oluşturulabilir.";
                return RedirectToAction("Detay", "SevkIrsaliyesi", new { id = irsaliyeId });
            }
            if (await _satisFaturasiService.AktifFaturaVarMiIrsaliyeIcinAsync(irsaliye.Id))
            {
                TempData["Hata"] = "Bu irsaliye için zaten bir fatura oluşturulmuş.";
                return RedirectToAction("Detay", "SevkIrsaliyesi", new { id = irsaliyeId });
            }

            var siparis = await _satisSiparisiService.GetByIdDetayAsync(irsaliye.SatisSiparisiId);
            var siparisFiyatlari = siparis?.Kalemler.ToDictionary(k => k.MalzemeId, k => (k.BirimFiyat, k.KdvOrani, k.Iskonto))
                ?? [];

            vm.CariId = irsaliye.SatisSiparisi.CariId;
            vm.SevkIrsaliyesiId = irsaliye.Id;
            vm.SatisSiparisiId = irsaliye.SatisSiparisiId;
            vm.IrsaliyeNoGosterim = irsaliye.IrsaliyeNo;
            vm.SiparisNoGosterim = irsaliye.SatisSiparisi.SiparisNo;
            vm.Kalemler = irsaliye.Kalemler.Select(k =>
            {
                var fiyatBulundu = siparisFiyatlari.TryGetValue(k.MalzemeId, out var fiyat);
                return new SatisFaturasiKalemFormViewModel
                {
                    MalzemeId = k.MalzemeId,
                    Miktar = k.Miktar,
                    BirimFiyat = fiyatBulundu ? fiyat.BirimFiyat : k.Malzeme.SatisFiyati,
                    KdvOrani = fiyatBulundu ? fiyat.KdvOrani : k.Malzeme.KdvOrani,
                    Iskonto = fiyatBulundu ? fiyat.Iskonto : 0
                };
            }).ToList();
        }
        else if (siparisId is not null)
        {
            var siparis = await _satisSiparisiService.GetByIdDetayAsync(siparisId.Value);
            if (siparis is null)
            {
                TempData["Hata"] = "Satış siparişi bulunamadı.";
                return RedirectToAction("Index", "SatisSiparisi");
            }
            if (siparis.Durum != BelgeDurum.Onaylandi)
            {
                TempData["Hata"] = "Fatura sadece onaylanmış bir siparişten oluşturulabilir.";
                return RedirectToAction("Detay", "SatisSiparisi", new { id = siparisId });
            }
            if (await _satisFaturasiService.AktifFaturaVarMiSiparisIcinAsync(siparis.Id))
            {
                TempData["Hata"] = "Bu sipariş için zaten bir fatura oluşturulmuş.";
                return RedirectToAction("Detay", "SatisSiparisi", new { id = siparisId });
            }

            vm.CariId = siparis.CariId;
            vm.SatisSiparisiId = siparis.Id;
            vm.SiparisNoGosterim = siparis.SiparisNo;
            vm.Kalemler = siparis.Kalemler.Select(k => new SatisFaturasiKalemFormViewModel
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
    public async Task<IActionResult> Ekle(SatisFaturasiFormViewModel vm)
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
            var fatura = new SatisFaturasi
            {
                Tarih = vm.Tarih,
                VadeTarihi = vm.VadeTarihi,
                CariId = vm.CariId!.Value,
                SevkIrsaliyesiId = vm.SevkIrsaliyesiId,
                SatisSiparisiId = vm.SatisSiparisiId,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new SatisFaturasiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani,
                Iskonto = k.Iskonto
            }).ToList();

            await _satisFaturasiService.CreateAsync(fatura, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Satış faturası kaydedildi.";
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
            await _satisFaturasiService.OnaylaAsync(id);
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
            await _satisFaturasiService.IptalEtAsync(id);
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
                    baslikSutunu.Item().Text("SakaryaERP").FontSize(18).Bold();
                    baslikSutunu.Item().PaddingTop(8).Text("Satış Faturası").FontSize(14).Bold();
                    baslikSutunu.Item().Text($"Fatura No: {vm.FaturaNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    if (vm.VadeTarihi is not null)
                        baslikSutunu.Item().Text($"Vade Tarihi: {vm.VadeTarihi:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Müşteri: {vm.CariUnvan}").FontSize(10);
                    if (vm.SiparisNo is not null)
                        baslikSutunu.Item().Text($"Sipariş No: {vm.SiparisNo}").FontSize(10);
                    if (vm.IrsaliyeNo is not null)
                        baslikSutunu.Item().Text($"İrsaliye No: {vm.IrsaliyeNo}").FontSize(10);
                });

                sayfa.Content().PaddingTop(10).Column(icerik =>
                {
                    icerik.Item().Table(tablo =>
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

                    icerik.Item().AlignRight().PaddingTop(10).Width(220).Column(ozet =>
                    {
                        ozet.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Ara Toplam");
                            r.ConstantItem(100).AlignRight().Text(vm.AraToplam.ToString("N2"));
                        });
                        ozet.Item().Row(r =>
                        {
                            r.RelativeItem().Text("İskonto");
                            r.ConstantItem(100).AlignRight().Text(vm.ToplamIskonto.ToString("N2"));
                        });
                        ozet.Item().Row(r =>
                        {
                            r.RelativeItem().Text("KDV");
                            r.ConstantItem(100).AlignRight().Text(vm.ToplamKdv.ToString("N2"));
                        });
                        ozet.Item().PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Medium).Row(r =>
                        {
                            r.RelativeItem().Text("Genel Toplam").Bold();
                            r.ConstantItem(100).AlignRight().Text(vm.ToplamTutar.ToString("N2")).Bold();
                        });
                    });
                });
            });
        });

        var dosyaAdi = $"satis-faturasi-{vm.FaturaNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<SatisFaturasiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var fatura = await _satisFaturasiService.GetByIdDetayAsync(id);
        if (fatura is null)
            return null;

        return new SatisFaturasiDetayViewModel
        {
            Id = fatura.Id,
            FaturaNo = fatura.FaturaNo,
            Tarih = fatura.Tarih,
            VadeTarihi = fatura.VadeTarihi,
            CariUnvan = fatura.Cari.Unvan,
            SiparisNo = fatura.SatisSiparisi?.SiparisNo,
            IrsaliyeNo = fatura.SevkIrsaliyesi?.IrsaliyeNo,
            Aciklama = fatura.Aciklama,
            Durum = fatura.Durum,
            DurumText = DurumMetni(fatura.Durum),
            Kalemler = fatura.Kalemler.Select(k => new SatisFaturasiKalemDetayViewModel
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

    private async Task DoldurListeler(SatisFaturasiFormViewModel vm)
    {
        var cariler = await _cariService.GetAllAsync();
        vm.MusteriListesi = cariler
            .Where(c => c.CariTipi is CariTipi.Musteri or CariTipi.HerIkisi)
            .Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        var malzemeler = await _malzemeService.GetTumListeAsync();
        ViewData["MalzemeListesiJson"] = malzemeler
            .Select(m => new { id = m.Id, kod = m.MalzemeKodu, ad = m.MalzemeAdi, birim = m.Birim });
    }

    private static decimal KalemToplami(SatisFaturasiKalemi k)
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
