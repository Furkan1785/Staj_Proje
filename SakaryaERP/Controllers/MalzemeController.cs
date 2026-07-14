using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class MalzemeController : Controller
{
    private readonly IMalzemeService _malzemeService;

    public MalzemeController(IMalzemeService malzemeService)
    {
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
        var siralamaSutunu = int.Parse(form["order[0][column]"].FirstOrDefault() ?? "0");
        var siralamaYonu = form["order[0][dir]"].FirstOrDefault() ?? "asc";

        var sutunAramalari = new string?[17];
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
}
