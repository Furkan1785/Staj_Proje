using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

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
