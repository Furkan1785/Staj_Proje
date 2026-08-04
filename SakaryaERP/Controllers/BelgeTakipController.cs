using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

// Kritik stok ve vadesi yaklaşan çek/senet için e-posta bildirimi zaten vardı (BildirimService)
// ama geçerlilik tarihi geçen teklifler / teslim tarihi geçen siparişler için kalıcı bir ekran
// yoktu — kullanıcı e-postayı kaçırırsa bu bilgiye bir daha ulaşamıyordu. Bu ekran o boşluğu
// dolduruyor: sadece Beklemede durumundaki (henüz sonuçlanmamış) belgelere bakar.
[Authorize(Roles = "Admin,Muhasebe,Satis")]
public class BelgeTakipController : Controller
{
    private readonly ISatisTeklifiService _satisTeklifiService;
    private readonly ISatisSiparisiService _satisSiparisiService;
    private readonly IAlisSiparisiService _alisSiparisiService;
    private readonly IConfiguration _configuration;

    public BelgeTakipController(
        ISatisTeklifiService satisTeklifiService,
        ISatisSiparisiService satisSiparisiService,
        IAlisSiparisiService alisSiparisiService,
        IConfiguration configuration)
    {
        _satisTeklifiService = satisTeklifiService;
        _satisSiparisiService = satisSiparisiService;
        _alisSiparisiService = alisSiparisiService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var gunSayisi = _configuration.GetValue<int?>("Bildirim:VadeYaklasmaGunSayisi") ?? 3;
        var bugun = DateTime.Today;
        var yaklasmaSiniri = bugun.AddDays(gunSayisi);

        var teklifler = await _satisTeklifiService.GetBeklemedeListesiAsync();
        var satisSiparisleri = await _satisSiparisiService.GetBeklemedeListesiAsync();
        var alisSiparisleri = await _alisSiparisiService.GetBeklemedeListesiAsync();

        var vm = new BelgeTakipViewModel { GunSayisi = gunSayisi };

        foreach (var t in teklifler.Where(t => t.GecerlilikTarihi is not null).OrderBy(t => t.GecerlilikTarihi))
        {
            var satir = new BelgeTakipSatiriViewModel
            {
                Id = t.Id,
                BelgeNo = t.TeklifNo,
                CariUnvan = t.Cari.Unvan,
                Tarih = t.GecerlilikTarihi!.Value,
                GunFarki = (t.GecerlilikTarihi.Value.Date - bugun).Days
            };
            if (satir.Tarih.Date < bugun) vm.GecikenTeklifler.Add(satir);
            else if (satir.Tarih.Date <= yaklasmaSiniri) vm.YaklasanTeklifler.Add(satir);
        }

        foreach (var s in satisSiparisleri.Where(s => s.TeslimTarihi is not null).OrderBy(s => s.TeslimTarihi))
        {
            var satir = new BelgeTakipSatiriViewModel
            {
                Id = s.Id,
                BelgeNo = s.SiparisNo,
                CariUnvan = s.Cari.Unvan,
                Tarih = s.TeslimTarihi!.Value,
                GunFarki = (s.TeslimTarihi.Value.Date - bugun).Days
            };
            if (satir.Tarih.Date < bugun) vm.GecikenSatisSiparisleri.Add(satir);
            else if (satir.Tarih.Date <= yaklasmaSiniri) vm.YaklasanSatisSiparisleri.Add(satir);
        }

        foreach (var s in alisSiparisleri.Where(s => s.TeslimTarihi is not null).OrderBy(s => s.TeslimTarihi))
        {
            var satir = new BelgeTakipSatiriViewModel
            {
                Id = s.Id,
                BelgeNo = s.SiparisNo,
                CariUnvan = s.Cari.Unvan,
                Tarih = s.TeslimTarihi!.Value,
                GunFarki = (s.TeslimTarihi.Value.Date - bugun).Days
            };
            if (satir.Tarih.Date < bugun) vm.GecikenAlisSiparisleri.Add(satir);
            else if (satir.Tarih.Date <= yaklasmaSiniri) vm.YaklasanAlisSiparisleri.Add(satir);
        }

        return View(vm);
    }
}
