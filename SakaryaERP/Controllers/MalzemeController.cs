using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe,Satis")]
public class MalzemeController : Controller
{
    private static readonly string[] IceAktarBasliklari =
    [
        "Malzeme Kodu", "Barkod", "Malzeme Adı", "Marka", "Kalite", "Tip", "Birim",
        "Kategori", "Temin Türü", "Stok Tipi", "Alış Fiyatı", "Satış Fiyatı",
        "KDV Oranı", "Min Stok", "Max Stok", "Raf No"
    ];

    private readonly IMalzemeService _malzemeService;
    private readonly IMalzemeKategoriService _malzemeKategoriService;
    private readonly IMalzemeHareketFisiService _malzemeHareketFisiService;
    private readonly ISatisFaturasiService _satisFaturasiService;
    private readonly IAlisFaturasiService _alisFaturasiService;
    private readonly ILogger<MalzemeController> _logger;

    public MalzemeController(
        IMalzemeService malzemeService,
        IMalzemeKategoriService malzemeKategoriService,
        IMalzemeHareketFisiService malzemeHareketFisiService,
        ISatisFaturasiService satisFaturasiService,
        IAlisFaturasiService alisFaturasiService,
        ILogger<MalzemeController> logger)
    {
        _malzemeService = malzemeService;
        _malzemeKategoriService = malzemeKategoriService;
        _malzemeHareketFisiService = malzemeHareketFisiService;
        _satisFaturasiService = satisFaturasiService;
        _alisFaturasiService = alisFaturasiService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var kritikStoklar = await _malzemeService.GetKritikStokListesiAsync();
        return View(kritikStoklar.Select(m => new KritikStokViewModel
        {
            Id = m.Id,
            MalzemeKodu = m.MalzemeKodu,
            MalzemeAdi = m.MalzemeAdi,
            Birim = m.Birim,
            Bakiye = m.Bakiye,
            MinStokMiktari = m.MinStokMiktari
        }).ToList());
    }

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

        var sutunAramalari = new string?[18];
        for (var i = 0; i < sutunAramalari.Length; i++)
            sutunAramalari[i] = form[$"columns[{i}][search][value]"].FirstOrDefault();

        var (kayitlar, toplamKayit, filtrelenmisKayit) = await _malzemeService.GetSayfaliListeAsync(
            start, length, genelArama, sutunAramalari, siralamaSutunu, siralamaYonu);

        var veri = kayitlar.Select(m => new MalzemeListItemViewModel
        {
            Id = m.Id,
            MalzemeKodu = m.MalzemeKodu,
            Barkod = m.Barkod,
            MalzemeAdi = m.MalzemeAdi,
            Marka = m.Marka,
            Kalite = m.Kalite,
            Tip = m.Tip,
            Birim = m.Birim,
            KategoriAdi = m.Kategori?.KategoriAdi,
            TeminTuruText = TeminTuruMetni(m.TeminTuru),
            StokTipiText = StokTipiMetni(m.StokTipi),
            AlisFiyati = m.AlisFiyati,
            SatisFiyati = m.SatisFiyati,
            KdvOrani = m.KdvOrani,
            MinStokMiktari = m.MinStokMiktari,
            MaxStokMiktari = m.MaxStokMiktari,
            RafNo = m.RafNo,
            Bakiye = m.Bakiye
        });

        return Json(new { draw, recordsTotal = toplamKayit, recordsFiltered = filtrelenmisKayit, data = veri });
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new MalzemeFormViewModel();
        await DoldurKategoriListesi(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(MalzemeFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurKategoriListesi(vm);
            return View(vm);
        }

        try
        {
            await _malzemeService.CreateAsync(VmDenMalzeme(vm));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.MalzemeKodu), ex.Message);
            await DoldurKategoriListesi(vm);
            return View(vm);
        }

        TempData["Basari"] = "Malzeme kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var malzeme = await _malzemeService.GetByIdAsync(id);
        if (malzeme is null) return NotFound();

        var vm = new MalzemeFormViewModel
        {
            Id = malzeme.Id,
            MalzemeKodu = malzeme.MalzemeKodu,
            Barkod = malzeme.Barkod,
            MalzemeAdi = malzeme.MalzemeAdi,
            Marka = malzeme.Marka,
            Kalite = malzeme.Kalite,
            Tip = malzeme.Tip,
            Birim = malzeme.Birim,
            KategoriId = malzeme.KategoriId,
            TeminTuru = malzeme.TeminTuru,
            StokTipi = malzeme.StokTipi,
            AlisFiyati = malzeme.AlisFiyati,
            SatisFiyati = malzeme.SatisFiyati,
            KdvOrani = malzeme.KdvOrani,
            MinStokMiktari = malzeme.MinStokMiktari,
            MaxStokMiktari = malzeme.MaxStokMiktari,
            RafNo = malzeme.RafNo
        };
        await DoldurKategoriListesi(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(MalzemeFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await DoldurKategoriListesi(vm);
            return View(vm);
        }

        try
        {
            var malzeme = VmDenMalzeme(vm);
            malzeme.Id = vm.Id;
            await _malzemeService.UpdateAsync(malzeme);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.MalzemeKodu), ex.Message);
            await DoldurKategoriListesi(vm);
            return View(vm);
        }

        TempData["Basari"] = "Malzeme güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var malzeme = await _malzemeService.GetByIdAsync(id);
        if (malzeme is null) return NotFound();

        var sonHareketler = await _malzemeHareketFisiService.GetMalzemeGecmisiAsync(id, null, null);
        var satisFaturalari = await _satisFaturasiService.GetOnaylanmisListeAsync();
        var alisFaturalari = await _alisFaturasiService.GetOnaylanmisListeAsync();

        return View(new MalzemeDetayViewModel
        {
            Id = malzeme.Id,
            MalzemeKodu = malzeme.MalzemeKodu,
            Barkod = malzeme.Barkod,
            MalzemeAdi = malzeme.MalzemeAdi,
            Marka = malzeme.Marka,
            Kalite = malzeme.Kalite,
            Tip = malzeme.Tip,
            Birim = malzeme.Birim,
            KategoriAdi = malzeme.Kategori?.KategoriAdi,
            TeminTuruText = TeminTuruMetni(malzeme.TeminTuru),
            StokTipiText = StokTipiMetni(malzeme.StokTipi),
            AlisFiyati = malzeme.AlisFiyati,
            SatisFiyati = malzeme.SatisFiyati,
            KdvOrani = malzeme.KdvOrani,
            MinStokMiktari = malzeme.MinStokMiktari,
            MaxStokMiktari = malzeme.MaxStokMiktari,
            RafNo = malzeme.RafNo,
            Bakiye = malzeme.Bakiye,
            SonHareketler = sonHareketler
                .OrderByDescending(k => k.MalzemeHareketFisi.Tarih)
                .Take(5)
                .Select(k => new MalzemeGecmisiSatiriViewModel
                {
                    Tarih = k.MalzemeHareketFisi.Tarih,
                    FisNo = k.MalzemeHareketFisi.FisNo,
                    HareketTipiText = HareketTipiMetni(k.MalzemeHareketFisi.HareketTipi),
                    SubeAdi = k.MalzemeHareketFisi.Sube.SubeAdi,
                    Giris = k.MalzemeHareketFisi.HareketTipi == HareketTipi.Giris ? k.Miktar : 0,
                    Cikis = k.MalzemeHareketFisi.HareketTipi is HareketTipi.Cikis or HareketTipi.Fire ? k.Miktar : 0,
                    Aciklama = k.Aciklama
                }).ToList(),
            SonSatislar = satisFaturalari
                .SelectMany(f => f.Kalemler.Where(k => k.MalzemeId == id).Select(k => new { Fatura = f, Kalem = k }))
                .OrderByDescending(x => x.Fatura.Tarih)
                .Take(5)
                .Select(x => new MalzemeFaturaSatiriViewModel
                {
                    FaturaId = x.Fatura.Id,
                    Tarih = x.Fatura.Tarih,
                    FaturaNo = x.Fatura.FaturaNo,
                    CariUnvan = x.Fatura.Cari.Unvan,
                    Miktar = x.Kalem.Miktar
                }).ToList(),
            SonAlislar = alisFaturalari
                .SelectMany(f => f.Kalemler.Where(k => k.MalzemeId == id).Select(k => new { Fatura = f, Kalem = k }))
                .OrderByDescending(x => x.Fatura.Tarih)
                .Take(5)
                .Select(x => new MalzemeFaturaSatiriViewModel
                {
                    FaturaId = x.Fatura.Id,
                    Tarih = x.Fatura.Tarih,
                    FaturaNo = x.Fatura.FaturaNo,
                    CariUnvan = x.Fatura.Cari.Unvan,
                    Miktar = x.Kalem.Miktar
                }).ToList()
        });
    }

    public async Task<IActionResult> Excel()
    {
        var malzemeler = await _malzemeService.GetTumListeAsync();

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Malzemeler");

        for (var i = 0; i < IceAktarBasliklari.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = IceAktarBasliklari[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }
        sayfa.Cell(1, 17).Value = "Bakiye";
        sayfa.Cell(1, 17).Style.Font.Bold = true;

        var satirNo = 2;
        foreach (var m in malzemeler)
        {
            sayfa.Cell(satirNo, 1).Value = m.MalzemeKodu;
            sayfa.Cell(satirNo, 2).Value = m.Barkod;
            sayfa.Cell(satirNo, 3).Value = m.MalzemeAdi;
            sayfa.Cell(satirNo, 4).Value = m.Marka;
            sayfa.Cell(satirNo, 5).Value = m.Kalite;
            sayfa.Cell(satirNo, 6).Value = m.Tip;
            sayfa.Cell(satirNo, 7).Value = m.Birim;
            sayfa.Cell(satirNo, 8).Value = m.Kategori?.KategoriAdi;
            sayfa.Cell(satirNo, 9).Value = TeminTuruMetni(m.TeminTuru);
            sayfa.Cell(satirNo, 10).Value = StokTipiMetni(m.StokTipi);
            sayfa.Cell(satirNo, 11).Value = m.AlisFiyati;
            sayfa.Cell(satirNo, 12).Value = m.SatisFiyati;
            sayfa.Cell(satirNo, 13).Value = m.KdvOrani;
            sayfa.Cell(satirNo, 14).Value = m.MinStokMiktari;
            sayfa.Cell(satirNo, 15).Value = m.MaxStokMiktari;
            sayfa.Cell(satirNo, 16).Value = m.RafNo;
            sayfa.Cell(satirNo, 17).Value = m.Bakiye;
            satirNo++;
        }
        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"malzeme-listesi-{DateTime.Today:yyyyMMdd}.xlsx");
    }

    public IActionResult IceAktarSablonu()
    {
        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Malzemeler");

        for (var i = 0; i < IceAktarBasliklari.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = IceAktarBasliklari[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }

        sayfa.Cell(2, 1).Value = "M100";
        sayfa.Cell(2, 3).Value = "Örnek Malzeme Adı";
        sayfa.Cell(2, 7).Value = "Adet";
        sayfa.Cell(2, 9).Value = "Alış";
        sayfa.Cell(2, 10).Value = "Ticari Mal";
        sayfa.Cell(2, 13).Value = 20;

        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "malzeme-ice-aktarma-sablonu.xlsx");
    }

    public IActionResult IceAktar() => View(new MalzemeIceAktarViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> IceAktar(IFormFile dosya)
    {
        if (dosya is null || dosya.Length == 0)
        {
            ModelState.AddModelError("", "Lütfen bir Excel dosyası (.xlsx) seçin.");
            return View(new MalzemeIceAktarViewModel());
        }

        // Sıkıştırılmış bir .xlsx açıldığında bellekte katbekat büyüyebilir (zip-bomb); dosya
        // boyutunu ve uzantısını/içerik tipini kontrol etmeden XLWorkbook'a vermek DoS riski taşır.
        const long maksimumBoyutBayt = 5 * 1024 * 1024;
        if (dosya.Length > maksimumBoyutBayt)
        {
            ModelState.AddModelError("", "Dosya boyutu 5 MB'ı aşamaz.");
            return View(new MalzemeIceAktarViewModel());
        }

        var uzanti = Path.GetExtension(dosya.FileName);
        if (!string.Equals(uzanti, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Sadece .xlsx uzantılı dosyalar kabul edilir.");
            return View(new MalzemeIceAktarViewModel());
        }

        var satirlar = new List<MalzemeImportSatiri>();
        try
        {
            using var stream = dosya.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var sayfa = workbook.Worksheet(1);
            var sonSatir = sayfa.LastRowUsed()?.RowNumber() ?? 1;
            for (var satirNo = 2; satirNo <= sonSatir; satirNo++)
            {
                var satir = sayfa.Row(satirNo);
                if (satir.IsEmpty()) continue;

                satirlar.Add(new MalzemeImportSatiri
                {
                    SatirNo = satirNo,
                    MalzemeKodu = satir.Cell(1).GetValue<string>(),
                    Barkod = satir.Cell(2).GetValue<string>(),
                    MalzemeAdi = satir.Cell(3).GetValue<string>(),
                    Marka = satir.Cell(4).GetValue<string>(),
                    Kalite = satir.Cell(5).GetValue<string>(),
                    Tip = satir.Cell(6).GetValue<string>(),
                    Birim = satir.Cell(7).GetValue<string>(),
                    KategoriAdi = satir.Cell(8).GetValue<string>(),
                    TeminTuru = satir.Cell(9).GetValue<string>(),
                    StokTipi = satir.Cell(10).GetValue<string>(),
                    AlisFiyati = satir.Cell(11).Value.ToString(),
                    SatisFiyati = satir.Cell(12).Value.ToString(),
                    KdvOrani = satir.Cell(13).Value.ToString(),
                    MinStokMiktari = satir.Cell(14).Value.ToString(),
                    MaxStokMiktari = satir.Cell(15).Value.ToString(),
                    RafNo = satir.Cell(16).GetValue<string>()
                });
            }
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            _logger.LogWarning(ex, "Malzeme içe aktarma dosyası okunamadı.");
            ModelState.AddModelError("", "Dosya okunamadı. Şablona uygun, bozulmamış bir .xlsx dosyası seçtiğinizden emin olun.");
            return View(new MalzemeIceAktarViewModel());
        }

        var sonuc = await _malzemeService.TopluIceAktarAsync(satirlar);

        if (sonuc.Hatalar.Count == 0)
        {
            TempData["Basari"] = $"{sonuc.BasariliSayisi} malzeme başarıyla içe aktarıldı.";
            return RedirectToAction(nameof(Index));
        }

        return View(new MalzemeIceAktarViewModel
        {
            BasariliSayisi = sonuc.BasariliSayisi,
            Hatalar = sonuc.Hatalar
        });
    }

    private async Task DoldurKategoriListesi(MalzemeFormViewModel vm)
    {
        var kategoriler = (await _malzemeKategoriService.GetAllAsync()).ToList();
        vm.KategoriListesi = KategoriSelectListiOlustur(kategoriler);
    }

    // Kategori ağacını (parentId ile) girinti eklenmiş düz bir listeye çeviriyor, dropdown'da
    // "Metal Ürünler" altında "— Boru ve Profil" gibi hiyerarşi görünsün diye.
    private static List<SelectListItem> KategoriSelectListiOlustur(List<MalzemeKategori> kategoriler)
    {
        var sonuc = new List<SelectListItem>();

        void Ekle(int? parentId, int seviye)
        {
            foreach (var kategori in kategoriler.Where(k => k.ParentId == parentId).OrderBy(k => k.KategoriAdi))
            {
                var onEk = seviye == 0 ? "" : new string(' ', (seviye - 1) * 2) + "— ";
                sonuc.Add(new SelectListItem($"{onEk}{kategori.KategoriAdi}", kategori.Id.ToString()));
                Ekle(kategori.Id, seviye + 1);
            }
        }

        Ekle(null, 0);
        return sonuc;
    }

    private static Malzeme VmDenMalzeme(MalzemeFormViewModel vm) => new()
    {
        MalzemeKodu = vm.MalzemeKodu,
        Barkod = vm.Barkod,
        MalzemeAdi = vm.MalzemeAdi,
        Marka = vm.Marka,
        Kalite = vm.Kalite,
        Tip = vm.Tip,
        Birim = vm.Birim,
        KategoriId = vm.KategoriId,
        TeminTuru = vm.TeminTuru,
        StokTipi = vm.StokTipi,
        AlisFiyati = vm.AlisFiyati,
        SatisFiyati = vm.SatisFiyati,
        KdvOrani = vm.KdvOrani,
        MinStokMiktari = vm.MinStokMiktari,
        MaxStokMiktari = vm.MaxStokMiktari,
        RafNo = vm.RafNo
    };

    private static string TeminTuruMetni(TeminTuru tur) => tur switch
    {
        TeminTuru.Alis => "Alış",
        TeminTuru.Uretim => "Üretim",
        TeminTuru.AlisUretim => "Alış+Üretim",
        _ => tur.ToString()
    };

    private static string StokTipiMetni(StokTipi tip) => tip switch
    {
        StokTipi.TicariMal => "Ticari Mal",
        StokTipi.Hammadde => "Hammadde",
        StokTipi.YariMamul => "Yarı Mamul",
        StokTipi.Mamul => "Mamul",
        _ => tip.ToString()
    };

    private static string HareketTipiMetni(HareketTipi tip) => tip switch
    {
        HareketTipi.Giris => "Giriş",
        HareketTipi.Cikis => "Çıkış",
        HareketTipi.Transfer => "Transfer",
        HareketTipi.Fire => "Fire",
        _ => tip.ToString()
    };
}
