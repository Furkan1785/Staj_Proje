using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Helpers;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class SatinalmaRaporuController : Controller
{
    private readonly IAlisFaturasiService _alisFaturasiService;

    public SatinalmaRaporuController(IAlisFaturasiService alisFaturasiService)
    {
        _alisFaturasiService = alisFaturasiService;
    }

    public async Task<IActionResult> Ozet()
    {
        var faturalar = await _alisFaturasiService.GetOnaylanmisListeAsync();
        var turkce = new CultureInfo("tr-TR");

        var vm = new SatinalmaOzetViewModel
        {
            GenelToplam = faturalar.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatirToplami)),
            TedarikciToplamlari = faturalar
                .GroupBy(f => f.Cari.Unvan)
                .Select(g => new TedarikciToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatirToplami)) })
                .OrderByDescending(t => t.ToplamTutar)
                .ToList(),
            AylikToplamlar = faturalar
                .GroupBy(f => new { f.Tarih.Year, f.Tarih.Month })
                .Select(g => new AylikToplamViewModel
                {
                    Yil = g.Key.Year,
                    Ay = g.Key.Month,
                    Etiket = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", turkce),
                    ToplamTutar = g.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatirToplami))
                })
                .OrderBy(a => a.Yil).ThenBy(a => a.Ay)
                .ToList()
        };

        return View(vm);
    }

}
