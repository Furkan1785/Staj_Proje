using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

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
            GenelToplam = faturalar.Sum(f => f.Kalemler.Sum(KalemToplami)),
            TedarikciToplamlari = faturalar
                .GroupBy(f => f.Cari.Unvan)
                .Select(g => new TedarikciToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(f => f.Kalemler.Sum(KalemToplami)) })
                .OrderByDescending(t => t.ToplamTutar)
                .ToList(),
            AylikToplamlar = faturalar
                .GroupBy(f => new { f.Tarih.Year, f.Tarih.Month })
                .Select(g => new AylikToplamViewModel
                {
                    Yil = g.Key.Year,
                    Ay = g.Key.Month,
                    Etiket = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", turkce),
                    ToplamTutar = g.Sum(f => f.Kalemler.Sum(KalemToplami))
                })
                .OrderBy(a => a.Yil).ThenBy(a => a.Ay)
                .ToList()
        };

        return View(vm);
    }

    private static decimal KalemToplami(AlisFaturasiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
    }
}
