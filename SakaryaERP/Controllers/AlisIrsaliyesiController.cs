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
public class AlisIrsaliyesiController : Controller
{
    private readonly IAlisIrsaliyesiService _alisIrsaliyesiService;
    private readonly IAlisSiparisiService _alisSiparisiService;
    private readonly ICariService _cariService;
    private readonly ISubeService _subeService;
    private readonly IMalzemeService _malzemeService;

    public AlisIrsaliyesiController(
        IAlisIrsaliyesiService alisIrsaliyesiService,
        IAlisSiparisiService alisSiparisiService,
        ICariService cariService,
        ISubeService subeService,
        IMalzemeService malzemeService)
    {
        _alisIrsaliyesiService = alisIrsaliyesiService;
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

        var sutunAramalari = new string?[7];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _alisIrsaliyesiService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(i => new AlisIrsaliyesiListItemViewModel
        {
            Id = i.Id,
            IrsaliyeNo = i.IrsaliyeNo,
            Tarih = i.Tarih,
            CariUnvan = i.Cari.Unvan,
            SubeAdi = i.Sube.SubeAdi,
            SiparisNo = i.AlisSiparisi?.SiparisNo ?? "-",
            Durum = i.Durum.ToString(),
            DurumText = DurumMetni(i.Durum),
            KalemSayisi = i.Kalemler.Count
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle(int? siparisId)
    {
        var vm = new AlisIrsaliyesiFormViewModel();

        if (siparisId is not null)
        {
            var siparis = await _alisSiparisiService.GetByIdDetayAsync(siparisId.Value);
            if (siparis is null)
            {
                TempData["Hata"] = "Alış siparişi bulunamadı.";
                return RedirectToAction("Index", "AlisSiparisi");
            }
            if (siparis.Durum != BelgeDurum.Onaylandi)
            {
                TempData["Hata"] = "İrsaliye sadece onaylanmış bir siparişten oluşturulabilir.";
                return RedirectToAction("Detay", "AlisSiparisi", new { id = siparisId });
            }

            var teslimMiktarlari = _alisSiparisiService.TeslimMiktarlariHesapla(siparis);
            var kalanKalemler = siparis.Kalemler
                .Select(k => new { k.MalzemeId, Kalan = k.Miktar - Math.Min(teslimMiktarlari.GetValueOrDefault(k.MalzemeId), k.Miktar) })
                .Where(k => k.Kalan > 0)
                .ToList();

            if (kalanKalemler.Count == 0)
            {
                TempData["Hata"] = "Bu siparişin tüm kalemleri zaten teslim alınmış.";
                return RedirectToAction("Detay", "AlisSiparisi", new { id = siparisId });
            }

            vm.CariId = siparis.CariId;
            vm.SubeId = siparis.SubeId;
            vm.AlisSiparisiId = siparis.Id;
            vm.SiparisNoGosterim = siparis.SiparisNo;
            vm.Kalemler = kalanKalemler
                .Select(k => new AlisIrsaliyesiKalemFormViewModel { MalzemeId = k.MalzemeId, Miktar = k.Kalan })
                .ToList();
        }

        await DoldurListeler(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(AlisIrsaliyesiFormViewModel vm)
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
            var irsaliye = new AlisIrsaliyesi
            {
                Tarih = vm.Tarih,
                CariId = vm.CariId!.Value,
                SubeId = vm.SubeId!.Value,
                AlisSiparisiId = vm.AlisSiparisiId,
                Aciklama = vm.Aciklama
            };
            var kalemler = vm.Kalemler.Select(k => new AlisIrsaliyesiKalemi
            {
                MalzemeId = k.MalzemeId!.Value,
                Miktar = k.Miktar
            }).ToList();

            await _alisIrsaliyesiService.CreateAsync(irsaliye, kalemler);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurListeler(vm);
            return View(vm);
        }

        TempData["Basari"] = "Alış irsaliyesi kaydedildi.";
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
            await _alisIrsaliyesiService.OnaylaAsync(id);
            TempData["Basari"] = "İrsaliye onaylandı, malzeme bakiyeleri güncellendi.";
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
            await _alisIrsaliyesiService.IptalEtAsync(id);
            TempData["Basari"] = "İrsaliye iptal edildi.";
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
                    baslikSutunu.Item().Text("Alış İrsaliyesi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"İrsaliye No: {vm.IrsaliyeNo}").FontSize(10);
                    baslikSutunu.Item().Text($"Tarih: {vm.Tarih:dd.MM.yyyy}").FontSize(10);
                    baslikSutunu.Item().Text($"Tedarikçi: {vm.CariUnvan}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(vm.SiparisNo))
                        baslikSutunu.Item().Text($"Sipariş No: {vm.SiparisNo}").FontSize(10);
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

        var dosyaAdi = $"alis-irsaliyesi-{vm.IrsaliyeNo}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private async Task<AlisIrsaliyesiDetayViewModel?> BuildDetayViewModel(int id)
    {
        var irsaliye = await _alisIrsaliyesiService.GetByIdDetayAsync(id);
        if (irsaliye is null)
            return null;

        return new AlisIrsaliyesiDetayViewModel
        {
            Id = irsaliye.Id,
            IrsaliyeNo = irsaliye.IrsaliyeNo,
            Tarih = irsaliye.Tarih,
            CariId = irsaliye.CariId,
            CariUnvan = irsaliye.Cari.Unvan,
            SubeAdi = irsaliye.Sube.SubeAdi,
            SiparisNo = irsaliye.AlisSiparisi?.SiparisNo,
            Aciklama = irsaliye.Aciklama,
            Durum = irsaliye.Durum,
            DurumText = DurumMetni(irsaliye.Durum),
            Zincir = BelgeZinciriOlustur(irsaliye),
            Kalemler = irsaliye.Kalemler.Select(k => new AlisIrsaliyesiKalemDetayViewModel
            {
                MalzemeKodu = k.Malzeme.MalzemeKodu,
                MalzemeAdi = k.Malzeme.MalzemeAdi,
                Birim = k.Malzeme.Birim,
                Miktar = k.Miktar
            }).ToList()
        };
    }

    private static IContainer PdfBaslikHucresi(IContainer container) =>
        container.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer PdfIcerikHucresi(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);

    private async Task DoldurListeler(AlisIrsaliyesiFormViewModel vm)
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

    private List<BelgeZinciriAdimi> BelgeZinciriOlustur(AlisIrsaliyesi irsaliye)
    {
        var zincir = new List<BelgeZinciriAdimi>();
        if (irsaliye.AlisSiparisi is not null)
        {
            zincir.Add(new BelgeZinciriAdimi
            {
                Etiket = "Sipariş",
                Metin = irsaliye.AlisSiparisi.SiparisNo,
                Href = Url.Action("Detay", "AlisSiparisi", new { id = irsaliye.AlisSiparisiId })
            });
        }
        zincir.Add(new BelgeZinciriAdimi { Etiket = "İrsaliye", Metin = irsaliye.IrsaliyeNo, Aktif = true });
        return zincir;
    }

    public async Task<IActionResult> Excel()
    {
        var (kayitlar, _, _) = await _alisIrsaliyesiService.GetSayfaliListeAsync(0, int.MaxValue, null, new string?[7], -1, "desc");

        string[] basliklar = ["İrsaliye No", "Tarih", "Cari", "Şube", "Sipariş No", "Durum", "Kalem Sayısı"];
        var satirlar = kayitlar.Select(irsaliye => new object?[]
        {
            irsaliye.IrsaliyeNo, irsaliye.Tarih, irsaliye.Cari.Unvan, irsaliye.Sube.SubeAdi,
            irsaliye.AlisSiparisi?.SiparisNo ?? "-", DurumMetni(irsaliye.Durum), irsaliye.Kalemler.Count
        });

        var dosya = ExcelYardimcisi.ListeOlustur("Alış İrsaliyeleri", basliklar, satirlar);
        return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"alis-irsaliyesi-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
    }
}
