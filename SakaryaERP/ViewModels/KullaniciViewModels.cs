using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class KullaniciListItemViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string AdSoyad { get; set; } = "";
    public string Roller { get; set; } = "";
    public string? SubeAdi { get; set; }
    public bool Aktif { get; set; }
}

public class KullaniciEkleViewModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre (Tekrar)")]
    public string SifreTekrar { get; set; } = "";

    [Display(Name = "Şube")]
    public int? SubeId { get; set; }

    [Display(Name = "Roller")]
    public List<string> SeciliRoller { get; set; } = [];

    public List<SelectListItem> SubeListesi { get; set; } = [];
    public List<string> RolListesi { get; set; } = [];
}

public class KullaniciDuzenleViewModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Display(Name = "Şube")]
    public int? SubeId { get; set; }

    [Display(Name = "Roller")]
    public List<string> SeciliRoller { get; set; } = [];

    public List<SelectListItem> SubeListesi { get; set; } = [];
    public List<string> RolListesi { get; set; } = [];
}

public class KullaniciSifreSifirlaViewModel
{
    public string Id { get; set; } = "";
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string YeniSifre { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare(nameof(YeniSifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    public string YeniSifreTekrar { get; set; } = "";
}
