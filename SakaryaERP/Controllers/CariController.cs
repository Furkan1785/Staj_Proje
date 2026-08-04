using ClosedXML.Excel;
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
public class CariController : Controller
{
    private readonly ICariService _cariService;
    private readonly ICariFisiService _cariFisiService;
    private readonly ICekSenetService _cekSenetService;

    public CariController(ICariService cariService, ICariFisiService cariFisiService, ICekSenetService cekSenetService)
    {
        _cariService = cariService;
        _cariFisiService = cariFisiService;
        _cekSenetService = cekSenetService;
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
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "0");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "asc";

        var sutunAramalari = new string?[7];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _cariService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(c => new CariListItemViewModel
        {
            Id = c.Id,
            CariKodu = c.CariKodu,
            Unvan = c.Unvan,
            CariTipiText = CariTipiMetni(c.CariTipi),
            Telefon = c.Telefon,
            EMail = c.EMail,
            Bakiye = c.Bakiye,
            IsDeleted = c.IsDeleted
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public IActionResult Ekle() => View(new CariFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(CariFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _cariService.CreateAsync(new Cari
            {
                CariKodu = vm.CariKodu,
                Unvan = vm.Unvan,
                CariTipi = vm.CariTipi,
                VergiNo = vm.VergiNo,
                Adres = vm.Adres,
                Telefon = vm.Telefon,
                EMail = vm.EMail,
                KrediLimiti = vm.KrediLimiti
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.CariKodu), ex.Message);
            return View(vm);
        }

        TempData["Basari"] = "Cari kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var cari = await _cariService.GetByIdAsync(id);
        if (cari is null) return NotFound();

        var baslangic = DateTime.Today.AddMonths(-3);
        var bitis = DateTime.Today;
        var (_, _, satirlar) = await _cariFisiService.GetEkstreAsync(id, baslangic, bitis);

        var sonHareketler = satirlar.TakeLast(10).Select(s => new CariEkstreSatiriViewModel
        {
            Tarih = s.Fis.Tarih,
            FisNo = s.Fis.FisNo,
            FisTipiText = FisTipiMetni(s.Fis.FisTipi),
            Aciklama = s.Fis.Aciklama,
            Borc = s.Fis.FisTipi == FisTipi.Borc ? s.Fis.Tutar : 0,
            Alacak = s.Fis.FisTipi != FisTipi.Borc ? s.Fis.Tutar : 0,
            KumulatifBakiye = s.KumulatifBakiye
        }).ToList();

        var cekSenetler = (await _cekSenetService.GetAllAsync())
            .Where(c => c.CariId == id && !c.IsDeleted)
            .OrderByDescending(c => c.VadeTarihi)
            .Select(c => new CariCekSenetSatiriViewModel
            {
                Id = c.Id,
                BelgeTipiText = BelgeTipiMetni(c.BelgeTipi),
                BelgeNo = c.BelgeNo,
                VadeTarihi = c.VadeTarihi,
                Tutar = c.Tutar,
                DurumText = CekSenetDurumMetni(c.Durum),
                DurumSinifi = CekSenetDurumSinifi(c.Durum)
            }).ToList();

        return View(new CariDetayViewModel
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipiText = CariTipiMetni(cari.CariTipi),
            VergiNo = cari.VergiNo,
            Adres = cari.Adres,
            Telefon = cari.Telefon,
            EMail = cari.EMail,
            Bakiye = cari.Bakiye,
            KrediLimiti = cari.KrediLimiti,
            IsDeleted = cari.IsDeleted,
            SonHareketler = sonHareketler,
            CekSenetler = cekSenetler
        });
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var cari = await _cariService.GetByIdAsync(id);
        if (cari is null) return NotFound();

        return View(new CariFormViewModel
        {
            Id = cari.Id,
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = cari.CariTipi,
            VergiNo = cari.VergiNo,
            Adres = cari.Adres,
            Telefon = cari.Telefon,
            EMail = cari.EMail,
            KrediLimiti = cari.KrediLimiti
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(CariFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _cariService.UpdateAsync(new Cari
            {
                Id = vm.Id,
                CariKodu = vm.CariKodu,
                Unvan = vm.Unvan,
                CariTipi = vm.CariTipi,
                VergiNo = vm.VergiNo,
                Adres = vm.Adres,
                Telefon = vm.Telefon,
                EMail = vm.EMail,
                KrediLimiti = vm.KrediLimiti
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.CariKodu), ex.Message);
            return View(vm);
        }

        TempData["Basari"] = "Cari güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(int id)
    {
        await _cariService.PasifYapAsync(id);
        TempData["Basari"] = "Cari pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(int id)
    {
        await _cariService.AktifEtAsync(id);
        TempData["Basari"] = "Cari aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Ekstre(int? cariId, DateTime? baslangic, DateTime? bitis)
    {
        var vm = new CariEkstreViewModel
        {
            CariId = cariId,
            Baslangic = baslangic ?? DateTime.Today.AddMonths(-3),
            Bitis = bitis ?? DateTime.Today
        };

        var cariler = await _cariService.GetAllAsync();
        vm.CariListesi = cariler.Select(c => new SelectListItem($"{c.CariKodu} - {c.Unvan}", c.Id.ToString()));

        if (cariId is not null)
        {
            var (cari, devirBakiye, satirlar) = await _cariFisiService.GetEkstreAsync(cariId.Value, vm.Baslangic, vm.Bitis);
            vm.CariUnvan = cari.Unvan;
            vm.DevirBakiye = devirBakiye;
            vm.Satirlar = satirlar.Select(s => new CariEkstreSatiriViewModel
            {
                Tarih = s.Fis.Tarih,
                FisNo = s.Fis.FisNo,
                FisTipiText = FisTipiMetni(s.Fis.FisTipi),
                Aciklama = s.Fis.Aciklama,
                Borc = s.Fis.FisTipi == FisTipi.Borc ? s.Fis.Tutar : 0,
                Alacak = s.Fis.FisTipi != FisTipi.Borc ? s.Fis.Tutar : 0,
                KumulatifBakiye = s.KumulatifBakiye
            }).ToList();
            vm.SonBakiye = vm.Satirlar.Count > 0 ? vm.Satirlar[^1].KumulatifBakiye : vm.DevirBakiye;
        }

        return View(vm);
    }

    public async Task<IActionResult> EkstreExcel(int cariId, DateTime baslangic, DateTime bitis)
    {
        var (cari, devirBakiye, satirlar) = await _cariFisiService.GetEkstreAsync(cariId, baslangic, bitis);

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Cari Ekstresi");

        sayfa.Cell(1, 1).Value = "Cari:";
        sayfa.Cell(1, 2).Value = $"{cari.CariKodu} - {cari.Unvan}";
        sayfa.Cell(2, 1).Value = "Tarih Aralığı:";
        sayfa.Cell(2, 2).Value = $"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";

        var basliklarSatiri = 4;
        string[] basliklar = ["Tarih", "Fiş No", "Fiş Tipi", "Açıklama", "Borç", "Alacak", "Bakiye"];
        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(basliklarSatiri, i + 1).Value = basliklar[i];
            sayfa.Cell(basliklarSatiri, i + 1).Style.Font.Bold = true;
        }

        var satirNo = basliklarSatiri + 1;
        sayfa.Cell(satirNo, 4).Value = "Devir Bakiyesi";
        sayfa.Cell(satirNo, 7).Value = devirBakiye;
        satirNo++;

        foreach (var s in satirlar)
        {
            sayfa.Cell(satirNo, 1).Value = s.Fis.Tarih;
            sayfa.Cell(satirNo, 1).Style.DateFormat.Format = "dd.MM.yyyy";
            sayfa.Cell(satirNo, 2).Value = s.Fis.FisNo;
            sayfa.Cell(satirNo, 3).Value = FisTipiMetni(s.Fis.FisTipi);
            sayfa.Cell(satirNo, 4).Value = s.Fis.Aciklama;
            sayfa.Cell(satirNo, 5).Value = s.Fis.FisTipi == FisTipi.Borc ? s.Fis.Tutar : 0;
            sayfa.Cell(satirNo, 6).Value = s.Fis.FisTipi != FisTipi.Borc ? s.Fis.Tutar : 0;
            sayfa.Cell(satirNo, 7).Value = s.KumulatifBakiye;
            satirNo++;
        }

        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var dosyaAdi = $"cari-ekstresi-{cari.CariKodu}-{baslangic:yyyyMMdd}-{bitis:yyyyMMdd}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
    }

    public async Task<IActionResult> EkstrePdf(int cariId, DateTime baslangic, DateTime bitis)
    {
        var (cari, devirBakiye, satirlar) = await _cariFisiService.GetEkstreAsync(cariId, baslangic, bitis);
        var sonBakiye = satirlar.Count > 0 ? satirlar[^1].KumulatifBakiye : devirBakiye;

        var belge = Document.Create(container =>
        {
            container.Page(sayfa =>
            {
                sayfa.Size(PageSizes.A4);
                sayfa.Margin(30);
                sayfa.DefaultTextStyle(x => x.FontSize(9));

                sayfa.Header().Column(baslikSutunu =>
                {
                    baslikSutunu.Item().Text("Cari Ekstresi").FontSize(16).Bold();
                    baslikSutunu.Item().Text($"{cari.CariKodu} - {cari.Unvan}").FontSize(11);
                    baslikSutunu.Item().Text($"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}").FontSize(10);
                });

                sayfa.Content().Table(tablo =>
                {
                    tablo.ColumnsDefinition(sutunlar =>
                    {
                        sutunlar.ConstantColumn(60);
                        sutunlar.ConstantColumn(70);
                        sutunlar.ConstantColumn(55);
                        sutunlar.RelativeColumn();
                        sutunlar.ConstantColumn(65);
                        sutunlar.ConstantColumn(65);
                        sutunlar.ConstantColumn(70);
                    });

                    tablo.Header(baslik =>
                    {
                        string[] basliklar = ["Tarih", "Fiş No", "Fiş Tipi", "Açıklama", "Borç", "Alacak", "Bakiye"];
                        foreach (var b in basliklar)
                            baslik.Cell().Element(PdfBaslikHucresi).Text(b);
                    });

                    tablo.Cell().ColumnSpan(4).Element(PdfDevirHucresi).Text("Devir Bakiyesi");
                    tablo.Cell().Element(PdfDevirHucresi).Text("");
                    tablo.Cell().Element(PdfDevirHucresi).Text("");
                    tablo.Cell().Element(PdfDevirHucresi).AlignRight().Text(devirBakiye.ToString("N2"));

                    foreach (var s in satirlar)
                    {
                        tablo.Cell().Element(PdfIcerikHucresi).Text(s.Fis.Tarih.ToString("dd.MM.yyyy"));
                        tablo.Cell().Element(PdfIcerikHucresi).Text(s.Fis.FisNo);
                        tablo.Cell().Element(PdfIcerikHucresi).Text(FisTipiMetni(s.Fis.FisTipi));
                        tablo.Cell().Element(PdfIcerikHucresi).Text(s.Fis.Aciklama ?? "");
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(s.Fis.FisTipi == FisTipi.Borc ? s.Fis.Tutar.ToString("N2") : "");
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(s.Fis.FisTipi != FisTipi.Borc ? s.Fis.Tutar.ToString("N2") : "");
                        tablo.Cell().Element(PdfIcerikHucresi).AlignRight().Text(s.KumulatifBakiye.ToString("N2"));
                    }
                });

                sayfa.Footer().AlignRight().Text($"Son Bakiye: {sonBakiye:N2}").Bold();
            });
        });

        var dosyaAdi = $"cari-ekstresi-{cari.CariKodu}-{baslangic:yyyyMMdd}-{bitis:yyyyMMdd}.pdf";
        return File(belge.GeneratePdf(), "application/pdf", dosyaAdi);
    }

    private static IContainer PdfBaslikHucresi(IContainer container) =>
        container.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer PdfDevirHucresi(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).Padding(4);

    private static IContainer PdfIcerikHucresi(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);

    public async Task<IActionResult> Excel()
    {
        var cariler = await _cariService.GetAllAsync();

        string[] basliklar = ["Cari Kodu", "Unvan", "Cari Tipi", "Vergi No", "Adres", "Telefon", "E-Posta", "Bakiye", "Kredi Limiti", "Durum"];
        var satirlar = cariler.Select(c => new object?[]
        {
            c.CariKodu, c.Unvan, CariTipiMetni(c.CariTipi), c.VergiNo, c.Adres, c.Telefon, c.EMail,
            c.Bakiye, c.KrediLimiti, c.IsDeleted ? "Pasif" : "Aktif"
        });

        var dosya = ExcelYardimcisi.ListeOlustur("Cariler", basliklar, satirlar);
        return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"cari-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
    }

    private static string CariTipiMetni(CariTipi tipi) => tipi switch
    {
        CariTipi.Musteri => "Müşteri",
        CariTipi.Tedarikci => "Tedarikçi",
        CariTipi.HerIkisi => "Her İkisi",
        _ => tipi.ToString()
    };

    private static string FisTipiMetni(FisTipi tipi) => tipi switch
    {
        FisTipi.Borc => "Borç",
        FisTipi.Alacak => "Alacak",
        FisTipi.Mahsup => "Mahsup",
        _ => tipi.ToString()
    };

    private static string BelgeTipiMetni(BelgeTipi tipi) => tipi switch
    {
        BelgeTipi.Cek => "Çek",
        BelgeTipi.Senet => "Senet",
        _ => tipi.ToString()
    };

    private static string CekSenetDurumMetni(CekSenetDurum durum) => durum switch
    {
        CekSenetDurum.Portfoyde => "Portföyde",
        CekSenetDurum.Tahsilde => "Tahsilde",
        CekSenetDurum.Ciro => "Ciro",
        CekSenetDurum.Karsiliksiz => "Karşılıksız",
        CekSenetDurum.TahsilEdildi => "Tahsil Edildi",
        _ => durum.ToString()
    };

    private static string CekSenetDurumSinifi(CekSenetDurum durum) => durum switch
    {
        CekSenetDurum.Portfoyde => "bg-primary",
        CekSenetDurum.Tahsilde => "bg-warning text-dark",
        CekSenetDurum.Ciro => "bg-info text-dark",
        CekSenetDurum.Karsiliksiz => "bg-danger",
        CekSenetDurum.TahsilEdildi => "bg-success",
        _ => "bg-secondary"
    };
}
