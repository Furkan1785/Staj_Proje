using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe")]
public class HesapPlaniController : Controller
{
    private readonly IHesapPlaniService _hesapPlaniService;

    public HesapPlaniController(IHesapPlaniService hesapPlaniService)
    {
        _hesapPlaniService = hesapPlaniService;
    }

    public async Task<IActionResult> Index()
    {
        var hesaplar = (await _hesapPlaniService.GetAllAsync()).ToList();
        var vm = HiyerarsikListeOlustur(hesaplar);
        return View(vm);
    }

    // Hesap ağacını (ParentId ile) girinti seviyesi eklenmiş düz bir listeye çeviriyor,
    // MalzemeController.KategoriSelectListiOlustur ile aynı yaklaşım.
    private static List<HesapPlaniListItemViewModel> HiyerarsikListeOlustur(List<HesapPlani> hesaplar)
    {
        var sonuc = new List<HesapPlaniListItemViewModel>();

        void Ekle(int? parentId, int seviye)
        {
            foreach (var hesap in hesaplar.Where(h => h.ParentId == parentId).OrderBy(h => h.HesapKodu))
            {
                sonuc.Add(new HesapPlaniListItemViewModel
                {
                    Id = hesap.Id,
                    HesapKodu = hesap.HesapKodu,
                    HesapAdi = hesap.HesapAdi,
                    HesapTipi = HesapTipiMetni(hesap.HesapTipi),
                    Seviye = seviye
                });
                Ekle(hesap.Id, seviye + 1);
            }
        }

        Ekle(null, 0);
        return sonuc;
    }

    public async Task<IActionResult> Excel()
    {
        var hesaplar = (await _hesapPlaniService.GetAllAsync()).ToList();
        var vm = HiyerarsikListeOlustur(hesaplar);

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Hesap Planı");

        string[] basliklar = ["Hesap Kodu", "Hesap Adı", "Hesap Tipi"];
        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = basliklar[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var satirNo = 2;
        foreach (var h in vm)
        {
            sayfa.Cell(satirNo, 1).Value = h.HesapKodu;
            sayfa.Cell(satirNo, 2).Value = new string(' ', h.Seviye * 2) + h.HesapAdi;
            sayfa.Cell(satirNo, 3).Value = h.HesapTipi;
            satirNo++;
        }
        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"hesap-plani-{DateTime.Today:yyyyMMdd}.xlsx");
    }

    private static string HesapTipiMetni(HesapTipi tipi) => tipi switch
    {
        HesapTipi.Aktif => "Aktif",
        HesapTipi.Pasif => "Pasif",
        HesapTipi.Gelir => "Gelir",
        HesapTipi.Gider => "Gider",
        HesapTipi.Ozkaynak => "Özkaynak",
        _ => tipi.ToString()
    };
}
