using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

// Kullanıcılar/roller daha önce sadece DbSeeder içinde sabit kodluydu; bu ekran Admin'in
// yeni kullanıcı açıp rol/şube ataması yapabilmesini ve gerektiğinde kullanıcıyı pasife
// alabilmesini (silmeden, IdentityUser'ın lockout mekanizmasıyla) sağlar.
[Authorize(Roles = "Admin")]
public class KullaniciController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ISubeService _subeService;

    public KullaniciController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ISubeService subeService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _subeService = subeService;
    }

    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _userManager.Users.Include(u => u.Sube).OrderBy(u => u.Email).ToListAsync();

        var vm = new List<KullaniciListItemViewModel>();
        foreach (var k in kullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(k);
            vm.Add(new KullaniciListItemViewModel
            {
                Id = k.Id,
                Email = k.Email ?? "",
                AdSoyad = k.AdSoyad,
                Roller = string.Join(", ", roller),
                SubeAdi = k.Sube?.SubeAdi,
                Aktif = !await _userManager.IsLockedOutAsync(k)
            });
        }

        return View(vm);
    }

    public async Task<IActionResult> Ekle()
    {
        var vm = new KullaniciEkleViewModel();
        await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(KullaniciEkleViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
            return View(vm);
        }

        var kullanici = new AppUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            AdSoyad = vm.AdSoyad,
            SubeId = vm.SubeId,
            EmailConfirmed = true
        };

        var sonuc = await _userManager.CreateAsync(kullanici, vm.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
            return View(vm);
        }

        if (vm.SeciliRoller.Count > 0)
            await _userManager.AddToRolesAsync(kullanici, vm.SeciliRoller);

        TempData["Basari"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(string id)
    {
        var kullanici = await _userManager.FindByIdAsync(id);
        if (kullanici is null) return NotFound();

        var vm = new KullaniciDuzenleViewModel
        {
            Id = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? "",
            SubeId = kullanici.SubeId,
            SeciliRoller = (await _userManager.GetRolesAsync(kullanici)).ToList()
        };
        await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(KullaniciDuzenleViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
            return View(vm);
        }

        var kullanici = await _userManager.FindByIdAsync(vm.Id);
        if (kullanici is null) return NotFound();

        if (!string.Equals(kullanici.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailSonuc = await _userManager.SetEmailAsync(kullanici, vm.Email);
            if (!emailSonuc.Succeeded)
            {
                foreach (var hata in emailSonuc.Errors)
                    ModelState.AddModelError("", hata.Description);
                await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
                return View(vm);
            }
            // Login e-posta üzerinden yapılıyor (UserName = Email kuralı) — e-posta
            // değiştiğinde kullanıcı adı da eşlenmezse eski adres girene kadar giriş yapılamaz.
            await _userManager.SetUserNameAsync(kullanici, vm.Email);
        }

        kullanici.AdSoyad = vm.AdSoyad;
        kullanici.SubeId = vm.SubeId;

        var guncelleSonuc = await _userManager.UpdateAsync(kullanici);
        if (!guncelleSonuc.Succeeded)
        {
            foreach (var hata in guncelleSonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            await ListeleriDoldur(vm.SubeListesi, vm.RolListesi);
            return View(vm);
        }

        var mevcutRoller = await _userManager.GetRolesAsync(kullanici);
        var eklenecek = vm.SeciliRoller.Except(mevcutRoller).ToList();
        var cikarilacak = mevcutRoller.Except(vm.SeciliRoller).ToList();
        if (eklenecek.Count > 0) await _userManager.AddToRolesAsync(kullanici, eklenecek);
        if (cikarilacak.Count > 0) await _userManager.RemoveFromRolesAsync(kullanici, cikarilacak);

        TempData["Basari"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> SifreSifirla(string id)
    {
        var kullanici = await _userManager.FindByIdAsync(id);
        if (kullanici is null) return NotFound();

        return View(new KullaniciSifreSifirlaViewModel { Id = kullanici.Id, AdSoyad = kullanici.AdSoyad });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SifreSifirla(KullaniciSifreSifirlaViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var kullanici = await _userManager.FindByIdAsync(vm.Id);
        if (kullanici is null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(kullanici);
        var sonuc = await _userManager.ResetPasswordAsync(kullanici, token, vm.YeniSifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            return View(vm);
        }

        TempData["Basari"] = "Şifre güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasifYap(string id)
    {
        if (id == _userManager.GetUserId(User))
        {
            TempData["Hata"] = "Kendi hesabınızı pasif yapamazsınız.";
            return RedirectToAction(nameof(Index));
        }

        var kullanici = await _userManager.FindByIdAsync(id);
        if (kullanici is null) return NotFound();

        await _userManager.SetLockoutEnabledAsync(kullanici, true);
        await _userManager.SetLockoutEndDateAsync(kullanici, DateTimeOffset.MaxValue);

        TempData["Basari"] = "Kullanıcı pasif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifEt(string id)
    {
        var kullanici = await _userManager.FindByIdAsync(id);
        if (kullanici is null) return NotFound();

        await _userManager.SetLockoutEndDateAsync(kullanici, null);

        TempData["Basari"] = "Kullanıcı aktif yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ListeleriDoldur(List<SelectListItem> subeListesi, List<string> rolListesi)
    {
        var subeler = await _subeService.GetAllAsync();
        subeListesi.Clear();
        subeListesi.AddRange(subeler.Where(s => !s.IsDeleted).Select(s => new SelectListItem(s.SubeAdi, s.Id.ToString())));

        rolListesi.Clear();
        rolListesi.AddRange(_roleManager.Roles.Select(r => r.Name!).OrderBy(r => r));
    }
}
