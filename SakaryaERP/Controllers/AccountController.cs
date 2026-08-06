using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SakaryaERP.Models;
using SakaryaERP.Services;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _environment = environment;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var sonuc = await _signInManager.PasswordSignInAsync(vm.Email, vm.Sifre, vm.BeniHatirla, lockoutOnFailure: true);
        if (sonuc.Succeeded)
        {
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (sonuc.IsLockedOut)
            ModelState.AddModelError("", "Hesap kilitli. Yöneticinizle iletişime geçin.");
        else
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var kullanici = await _userManager.FindByEmailAsync(vm.Email);
        if (kullanici is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(kullanici);
            var link = SifreSifirlaLinkiOlustur(vm.Email, token);

            await _emailSender.SendEmailAsync(vm.Email, "TicariSistem Şifre Sıfırlama",
                $"<p>Şifrenizi sıfırlamak için <a href=\"{link}\">buraya tıklayın</a>.</p>" +
                "<p>Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>");
        }

        // Kayıtlı olup olmadığını belli etmemek için sonuç her durumda aynı.
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    public IActionResult ForgotPasswordConfirmation() => View();

    // AllowedHosts "*" olduğu için Request.Host güvenilir değil — bir saldırgan Host header'ını
    // kendi domainine çevirip gerçek (geçerli) bir sıfırlama token'ını kendi sunucusuna
    // yönlendirebilir (host header injection → hesap ele geçirme). Bu yüzden link, Request.Host
    // yerine appsettings/user-secrets'te açıkça tanımlanan App:BaseUrl'den kurulur. Sadece
    // Development'ta (internet'e açık olmayan, düşük riskli yerel test ortamı) BaseUrl
    // tanımlanmamışsa Request bilgisine düşülür; Production'da BaseUrl zorunludur.
    private string SifreSifirlaLinkiOlustur(string email, string token)
    {
        var relativePath = Url.Action(nameof(ResetPassword), "Account", new { email, token })!;
        var baseUrl = _configuration["App:BaseUrl"];

        if (!string.IsNullOrWhiteSpace(baseUrl))
            return $"{baseUrl.TrimEnd('/')}{relativePath}";

        if (_environment.IsDevelopment())
            return Url.Action(nameof(ResetPassword), "Account", new { email, token }, protocol: Request.Scheme)!;

        throw new InvalidOperationException(
            "App:BaseUrl yapılandırılmamış — güvenlik nedeniyle Production'da şifre sıfırlama linki Host header'ından kurulmaz.");
    }

    public IActionResult ResetPassword(string? email, string? token)
    {
        if (email is null || token is null)
            return RedirectToAction("Index", "Home");

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var kullanici = await _userManager.FindByEmailAsync(vm.Email);
        if (kullanici is null)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        var sonuc = await _userManager.ResetPasswordAsync(kullanici, vm.Token, vm.YeniSifre);
        if (sonuc.Succeeded)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        foreach (var hata in sonuc.Errors)
            ModelState.AddModelError("", hata.Description);

        return View(vm);
    }

    public IActionResult ResetPasswordConfirmation() => View();
}
