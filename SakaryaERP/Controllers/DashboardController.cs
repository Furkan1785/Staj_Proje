using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IMalzemeKategoriService _malzemeKategoriService;

    public DashboardController(IDashboardService dashboardService, IMalzemeKategoriService malzemeKategoriService)
    {
        _dashboardService = dashboardService;
        _malzemeKategoriService = malzemeKategoriService;
    }

    public async Task<IActionResult> Index(DashboardGorunumTuru tur = DashboardGorunumTuru.Satis,
        DashboardAralik aralik = DashboardAralik.TumZamanlar, int[]? kategoriId = null)
    {
        var seciliKategoriIdler = kategoriId?.ToList() ?? [];
        var kategoriler = (await _malzemeKategoriService.GetAllAsync()).ToList();

        // Bir üst kategori seçildiğinde alt kategorilerindeki malzemeler de dahil olsun —
        // malzemeler genelde ağacın yaprak (en alt) düğümüne atanıyor, sadece üst kategori
        // id'siyle birebir eşleştirme yapsaydık üst kategori seçimi hep boş sonuç verirdi.
        var genisletilmisKategoriIdler = KategoriVeAltlariniGenislet(seciliKategoriIdler, kategoriler);

        var vm = await _dashboardService.GetDashboardAsync(aralik, genisletilmisKategoriIdler);
        vm.SeciliTur = tur;
        vm.SeciliAralik = aralik;
        vm.SeciliKategoriIdler = seciliKategoriIdler;
        vm.KategoriFiltreListesi = KategoriFiltreListesiOlustur(kategoriler);

        return View(vm);
    }

    private static List<int> KategoriVeAltlariniGenislet(List<int> seciliIdler, List<MalzemeKategori> kategoriler)
    {
        if (seciliIdler.Count == 0) return seciliIdler;

        var sonuc = new HashSet<int>(seciliIdler);

        void AltlariEkle(int parentId)
        {
            foreach (var alt in kategoriler.Where(k => k.ParentId == parentId))
            {
                if (sonuc.Add(alt.Id))
                    AltlariEkle(alt.Id);
            }
        }

        foreach (var id in seciliIdler)
            AltlariEkle(id);

        return sonuc.ToList();
    }

    // Kategori ağacını (parentId ile) girinti eklenmiş düz bir listeye çeviriyor —
    // MalzemeController'daki dropdown listesiyle aynı mantık, burada checkbox listesi için.
    private static List<KategoriFiltreViewModel> KategoriFiltreListesiOlustur(List<MalzemeKategori> kategoriler)
    {
        var sonuc = new List<KategoriFiltreViewModel>();

        void Ekle(int? parentId, int seviye)
        {
            foreach (var kategori in kategoriler.Where(k => k.ParentId == parentId).OrderBy(k => k.KategoriAdi))
            {
                sonuc.Add(new KategoriFiltreViewModel { Id = kategori.Id, KategoriAdi = kategori.KategoriAdi, Seviye = seviye });
                Ekle(kategori.Id, seviye + 1);
            }
        }

        Ekle(null, 0);
        return sonuc;
    }
}
