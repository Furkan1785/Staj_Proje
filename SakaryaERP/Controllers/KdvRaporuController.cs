using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Helpers;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

// Basit dönemsel KDV özeti: Hesaplanan KDV (satış faturaları) vs İndirilecek KDV
// (alış faturaları). Beyanname üretmez, sadece "bu dönem ne kadar KDV ödenecek/
// devredecek" sorusuna cevap verir (CLAUDE.md'de kapsam dışı bırakılan "beyannameler"
// maddesinin ötesine geçmez).
[Authorize(Roles = "Admin,Muhasebe")]
public class KdvRaporuController : Controller
{
    private readonly ISatisFaturasiService _satisFaturasiService;
    private readonly IAlisFaturasiService _alisFaturasiService;

    public KdvRaporuController(ISatisFaturasiService satisFaturasiService, IAlisFaturasiService alisFaturasiService)
    {
        _satisFaturasiService = satisFaturasiService;
        _alisFaturasiService = alisFaturasiService;
    }

    public async Task<IActionResult> Ozet(DateTime? baslangic, DateTime? bitis)
    {
        var araligBaslangic = baslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var araligBitis = bitis ?? araligBaslangic.AddMonths(1).AddDays(-1);

        var satisFaturalari = (await _satisFaturasiService.GetOnaylanmisListeAsync())
            .Where(f => f.Tarih.Date >= araligBaslangic.Date && f.Tarih.Date <= araligBitis.Date)
            .ToList();
        var alisFaturalari = (await _alisFaturasiService.GetOnaylanmisListeAsync())
            .Where(f => f.Tarih.Date >= araligBaslangic.Date && f.Tarih.Date <= araligBitis.Date)
            .ToList();

        var turkce = new CultureInfo("tr-TR");
        var aylikSatis = satisFaturalari
            .GroupBy(f => new { f.Tarih.Year, f.Tarih.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatisFaturasiKdvTutari)));
        var aylikAlis = alisFaturalari
            .GroupBy(f => new { f.Tarih.Year, f.Tarih.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(f => f.Kalemler.Sum(FinansHesaplama.KdvTutari)));

        var aylar = aylikSatis.Keys.Union(aylikAlis.Keys).OrderBy(a => a.Year).ThenBy(a => a.Month);

        var vm = new KdvOzetViewModel
        {
            Baslangic = araligBaslangic,
            Bitis = araligBitis,
            HesaplananKdv = satisFaturalari.Sum(f => f.Kalemler.Sum(FinansHesaplama.SatisFaturasiKdvTutari)),
            IndirilecekKdv = alisFaturalari.Sum(f => f.Kalemler.Sum(FinansHesaplama.KdvTutari)),
            AylikKirilim = aylar.Select(a => new AylikKdvViewModel
            {
                Yil = a.Year,
                Ay = a.Month,
                Etiket = new DateTime(a.Year, a.Month, 1).ToString("MMM yyyy", turkce),
                HesaplananKdv = aylikSatis.GetValueOrDefault(a),
                IndirilecekKdv = aylikAlis.GetValueOrDefault(a)
            }).ToList()
        };

        return View(vm);
    }
}
