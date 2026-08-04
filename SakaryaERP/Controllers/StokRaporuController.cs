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

    public StokRaporuController(IMalzemeService malzemeService, IMalzemeHareketFisiService malzemeHareketFisiService)
    {
        _malzemeService = malzemeService;
        _malzemeHareketFisiService = malzemeHareketFisiService;
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

    public async Task<IActionResult> MalzemeGecmisi(int? malzemeId, DateTime? baslangic, DateTime? bitis)
    {
        var vm = new MalzemeGecmisiViewModel { MalzemeId = malzemeId, Baslangic = baslangic, Bitis = bitis };

        var malzemeler = await _malzemeService.GetTumListeAsync();
        vm.MalzemeListesi = malzemeler.Select(m => new SelectListItem($"{m.MalzemeKodu} - {m.MalzemeAdi}", m.Id.ToString()));

        if (malzemeId is not null)
        {
            var malzeme = await _malzemeService.GetByIdAsync(malzemeId.Value);
            if (malzeme is null) return NotFound();

            vm.MalzemeAdi = malzeme.MalzemeAdi;
            vm.Birim = malzeme.Birim;
            vm.GuncelBakiye = malzeme.Bakiye;

            var kalemler = await _malzemeHareketFisiService.GetMalzemeGecmisiAsync(malzemeId.Value, baslangic, bitis);
            vm.Satirlar = kalemler.Select(k => new MalzemeGecmisiSatiriViewModel
            {
                Tarih = k.MalzemeHareketFisi.Tarih,
                FisNo = k.MalzemeHareketFisi.FisNo,
                HareketTipiText = HareketTipiMetni(k.MalzemeHareketFisi.HareketTipi),
                SubeAdi = k.MalzemeHareketFisi.Sube.SubeAdi,
                Giris = k.MalzemeHareketFisi.HareketTipi == HareketTipi.Giris ? k.Miktar : 0,
                Cikis = k.MalzemeHareketFisi.HareketTipi is HareketTipi.Cikis or HareketTipi.Fire ? k.Miktar : 0,
                Aciklama = k.Aciklama
            }).ToList();

            vm.ToplamGiris = vm.Satirlar.Sum(s => s.Giris);
            vm.ToplamCikis = vm.Satirlar.Sum(s => s.Cikis);
        }

        return View(vm);
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
