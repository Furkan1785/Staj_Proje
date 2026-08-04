using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Helpers;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Satis")]
public class SatisRaporuController : Controller
{
    private readonly ISatisFaturasiService _satisFaturasiService;

    public SatisRaporuController(ISatisFaturasiService satisFaturasiService)
    {
        _satisFaturasiService = satisFaturasiService;
    }

    public async Task<IActionResult> Ozet()
    {
        var faturalar = await _satisFaturasiService.GetOnaylanmisListeAsync();
        var turkce = new CultureInfo("tr-TR");

        var vm = new SatisOzetViewModel
        {
            GenelToplam = faturalar.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami)),
            MusteriToplamlari = faturalar
                .GroupBy(f => f.Cari.Unvan)
                .Select(g => new MusteriToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami)) })
                .OrderByDescending(t => t.ToplamTutar)
                .ToList(),
            MalzemeToplamlari = faturalar
                .SelectMany(f => f.Kalemler)
                .GroupBy(k => k.Malzeme.MalzemeAdi)
                .Select(g => new MalzemeToplamViewModel { MalzemeAdi = g.Key, ToplamTutar = g.Sum(FinansHesaplama.SatisFaturasiSatirToplami) })
                .OrderByDescending(t => t.ToplamTutar)
                .ToList(),
            AylikToplamlar = faturalar
                .GroupBy(f => new { f.Tarih.Year, f.Tarih.Month })
                .Select(g => new AylikToplamViewModel
                {
                    Yil = g.Key.Year,
                    Ay = g.Key.Month,
                    Etiket = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", turkce),
                    ToplamTutar = g.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami))
                })
                .OrderBy(a => a.Yil).ThenBy(a => a.Ay)
                .ToList()
        };

        return View(vm);
    }

}
