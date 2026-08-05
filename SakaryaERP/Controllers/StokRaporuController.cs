using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[Authorize(Roles = "Admin,Muhasebe,Satis")]
public class StokRaporuController : Controller
{
    private readonly IMalzemeService _malzemeService;
    private readonly IMalzemeHareketFisiService _malzemeHareketFisiService;
    private readonly ISevkIrsaliyesiService _sevkIrsaliyesiService;
    private readonly IAlisIrsaliyesiService _alisIrsaliyesiService;
    private readonly ISatisFaturasiService _satisFaturasiService;
    private readonly IAlisFaturasiService _alisFaturasiService;
    private readonly ISubeService _subeService;
    private readonly IOnayYetkisiService _onayYetkisiService;

    public StokRaporuController(
        IMalzemeService malzemeService,
        IMalzemeHareketFisiService malzemeHareketFisiService,
        ISevkIrsaliyesiService sevkIrsaliyesiService,
        IAlisIrsaliyesiService alisIrsaliyesiService,
        ISatisFaturasiService satisFaturasiService,
        IAlisFaturasiService alisFaturasiService,
        ISubeService subeService,
        IOnayYetkisiService onayYetkisiService)
    {
        _malzemeService = malzemeService;
        _malzemeHareketFisiService = malzemeHareketFisiService;
        _sevkIrsaliyesiService = sevkIrsaliyesiService;
        _alisIrsaliyesiService = alisIrsaliyesiService;
        _satisFaturasiService = satisFaturasiService;
        _alisFaturasiService = alisFaturasiService;
        _subeService = subeService;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<IActionResult> AnlikDurum()
    {
        var malzemeler = await _malzemeService.GetTumListeAsync();
        return View(malzemeler.Select(MalzemeyiVmYap).ToList());
    }

    public async Task<IActionResult> KritikStok()
    {
        var malzemeler = await _malzemeService.GetKritikStokListesiAsync();
        return View(malzemeler.Select(MalzemeyiVmYap).ToList());
    }

    public async Task<IActionResult> MalzemeGecmisi(int? malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId)
    {
        // Şubeye bağlı kullanıcı query string'e başka bir subeId yazarak başka şubenin
        // kardeksini göremesin diye istenen değer değil, etkin (yetkiye göre sabitlenmiş) değer kullanılıyor.
        var etkinSubeId = _onayYetkisiService.EfektifSube(subeId);
        var vm = new MalzemeGecmisiViewModel { MalzemeId = malzemeId, Baslangic = baslangic, Bitis = bitis, SubeId = etkinSubeId };

        var malzemeler = await _malzemeService.GetTumListeAsync();
        vm.MalzemeListesi = malzemeler.Select(m => new SelectListItem($"{m.MalzemeKodu} - {m.MalzemeAdi}", m.Id.ToString()));

        var subeler = await _subeService.GetAllAsync();
        var kullaniciSubeId = _onayYetkisiService.MevcutKullaniciSubeId();
        var gorunurSubeler = User.IsInRole("Admin") || kullaniciSubeId is null
            ? subeler.Where(s => !s.IsDeleted)
            : subeler.Where(s => !s.IsDeleted && s.Id == kullaniciSubeId);
        vm.SubeListesi = gorunurSubeler.Select(s => new SelectListItem(s.SubeAdi, s.Id.ToString()));

        if (malzemeId is not null)
        {
            var malzeme = await _malzemeService.GetByIdAsync(malzemeId.Value);
            if (malzeme is null) return NotFound();

            vm.MalzemeAdi = malzeme.MalzemeAdi;
            vm.Birim = malzeme.Birim;
            vm.GuncelBakiye = malzeme.Bakiye;

            if (vm.KumulatifBakiyeGosterilebilir)
            {
                // Kümülatif bakiyenin GuncelBakiye ile uyuşması için tarih filtresi olmaksızın TÜM
                // geçmiş çekilir (Malzeme oluşturulurken elle girilen bir açılış bakiyesi olabiliyor,
                // bkz. DemoSeeder — bu yüzden 0'dan ileri doğru değil, bilinen GuncelBakiye'den geriye
                // doğru yürütülür), sonra sadece görüntüleme için tarih aralığı uygulanır.
                var tumSatirlar = await TumHareketleriGetirAsync(malzemeId.Value, null, null, null);
                tumSatirlar = tumSatirlar.OrderByDescending(s => s.Tarih).ToList();

                var bakiye = malzeme.Bakiye;
                foreach (var satir in tumSatirlar)
                {
                    satir.KumulatifBakiye = bakiye;
                    bakiye -= satir.Giris - satir.Cikis;
                }
                vm.DevirBakiye = bakiye;

                vm.Satirlar = tumSatirlar
                    .Where(s => (baslangic is null || s.Tarih >= baslangic) && (bitis is null || s.Tarih <= bitis))
                    .OrderBy(s => s.Tarih)
                    .ToList();
            }
            else
            {
                vm.Satirlar = (await TumHareketleriGetirAsync(malzemeId.Value, baslangic, bitis, etkinSubeId))
                    .OrderBy(s => s.Tarih)
                    .ToList();
            }

            vm.ToplamGiris = vm.Satirlar.Sum(s => s.Giris);
            vm.ToplamCikis = vm.Satirlar.Sum(s => s.Cikis);
        }

        return View(vm);
    }

    // Kardeks: Malzeme.Bakiye'yi etkileyen dört ayrı kaynağı (manuel hareket fişi, sevk/alış
    // irsaliyesi, irsaliyesiz doğrudan satış/alış faturası) tek bir kronolojik listede birleştirir.
    // İrsaliyeli faturaların kalemleri hariç tutulur (o stok hareketi zaten irsaliye tarafında
    // sayılıyor — aksi halde aynı hareket iki kez sayılırdı).
    private async Task<List<MalzemeGecmisiSatiriViewModel>> TumHareketleriGetirAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId)
    {
        var hareketFisiKalemleri = await _malzemeHareketFisiService.GetMalzemeGecmisiAsync(malzemeId, baslangic, bitis, subeId);
        var sevkKalemleri = await _sevkIrsaliyesiService.GetMalzemeHareketleriAsync(malzemeId, baslangic, bitis, subeId);
        var alisIrsaliyeKalemleri = await _alisIrsaliyesiService.GetMalzemeHareketleriAsync(malzemeId, baslangic, bitis, subeId);
        var dogrudanSatisKalemleri = await _satisFaturasiService.GetMalzemeDogrudanSatisHareketleriAsync(malzemeId, baslangic, bitis, subeId);
        var dogrudanAlisKalemleri = await _alisFaturasiService.GetMalzemeDogrudanAlisHareketleriAsync(malzemeId, baslangic, bitis, subeId);

        var satirlar = new List<MalzemeGecmisiSatiriViewModel>();

        satirlar.AddRange(hareketFisiKalemleri.Select(k => new MalzemeGecmisiSatiriViewModel
        {
            Tarih = k.MalzemeHareketFisi.Tarih,
            FisNo = k.MalzemeHareketFisi.FisNo,
            HareketTipiText = HareketTipiMetni(k.MalzemeHareketFisi.HareketTipi),
            SubeAdi = k.MalzemeHareketFisi.Sube.SubeAdi,
            Giris = k.MalzemeHareketFisi.HareketTipi == HareketTipi.Giris ? k.Miktar : 0,
            Cikis = k.MalzemeHareketFisi.HareketTipi is HareketTipi.Cikis or HareketTipi.Fire ? k.Miktar : 0,
            Aciklama = k.Aciklama
        }));

        satirlar.AddRange(sevkKalemleri.Select(k => new MalzemeGecmisiSatiriViewModel
        {
            Tarih = k.SevkIrsaliyesi.Tarih,
            FisNo = k.SevkIrsaliyesi.IrsaliyeNo,
            HareketTipiText = "Satış (Sevk İrsaliyesi)",
            SubeAdi = k.SevkIrsaliyesi.Sube.SubeAdi,
            Giris = 0,
            Cikis = k.Miktar
        }));

        satirlar.AddRange(alisIrsaliyeKalemleri.Select(k => new MalzemeGecmisiSatiriViewModel
        {
            Tarih = k.AlisIrsaliyesi.Tarih,
            FisNo = k.AlisIrsaliyesi.IrsaliyeNo,
            HareketTipiText = "Alış (Alış İrsaliyesi)",
            SubeAdi = k.AlisIrsaliyesi.Sube.SubeAdi,
            Giris = k.Miktar,
            Cikis = 0
        }));

        satirlar.AddRange(dogrudanSatisKalemleri.Select(k => new MalzemeGecmisiSatiriViewModel
        {
            Tarih = k.SatisFaturasi.Tarih,
            FisNo = k.SatisFaturasi.FaturaNo,
            HareketTipiText = "Satış (Doğrudan Fatura)",
            SubeAdi = k.SatisFaturasi.Sube?.SubeAdi ?? "-",
            Giris = 0,
            Cikis = k.Miktar
        }));

        satirlar.AddRange(dogrudanAlisKalemleri.Select(k => new MalzemeGecmisiSatiriViewModel
        {
            Tarih = k.AlisFaturasi.Tarih,
            FisNo = k.AlisFaturasi.FaturaNo,
            HareketTipiText = "Alış (Doğrudan Fatura)",
            SubeAdi = k.AlisFaturasi.Sube?.SubeAdi ?? "-",
            Giris = k.Miktar,
            Cikis = 0
        }));

        return satirlar;
    }

    private static AnlikStokViewModel MalzemeyiVmYap(Malzeme m)
    {
        var (durumText, durumSinifi) = StokDurumu(m.Bakiye, m.MinStokMiktari, m.MaxStokMiktari);
        return new AnlikStokViewModel
        {
            Id = m.Id,
            MalzemeKodu = m.MalzemeKodu,
            MalzemeAdi = m.MalzemeAdi,
            KategoriAdi = m.Kategori?.KategoriAdi,
            Birim = m.Birim,
            Bakiye = m.Bakiye,
            MinStokMiktari = m.MinStokMiktari,
            MaxStokMiktari = m.MaxStokMiktari,
            DurumText = durumText,
            DurumSinifi = durumSinifi
        };
    }

    private static (string Text, string Sinif) StokDurumu(decimal bakiye, decimal min, decimal max)
    {
        if (bakiye < min) return ("Kritik Stok", "bg-danger");
        if (bakiye > max) return ("Stok Fazlası", "bg-warning text-dark");
        return ("Normal", "bg-success");
    }

    private static string HareketTipiMetni(HareketTipi tip) => tip switch
    {
        HareketTipi.Giris => "Giriş",
        HareketTipi.Cikis => "Çıkış",
        HareketTipi.Transfer => "Transfer",
        HareketTipi.Fire => "Fire",
        _ => tip.ToString()
    };
}
