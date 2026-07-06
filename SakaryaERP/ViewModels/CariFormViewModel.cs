using System.ComponentModel.DataAnnotations;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class CariFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Cari kodu zorunludur.")]
    [StringLength(30, ErrorMessage = "Cari kodu en fazla 30 karakter olabilir.")]
    [Display(Name = "Cari Kodu")]
    public string CariKodu { get; set; } = "";

    [Required(ErrorMessage = "Unvan zorunludur.")]
    [StringLength(200, ErrorMessage = "Unvan en fazla 200 karakter olabilir.")]
    [Display(Name = "Unvan")]
    public string Unvan { get; set; } = "";

    [Display(Name = "Cari Tipi")]
    public CariTipi CariTipi { get; set; }

    [StringLength(20, ErrorMessage = "Vergi no en fazla 20 karakter olabilir.")]
    [Display(Name = "Vergi No")]
    public string? VergiNo { get; set; }

    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [Display(Name = "E-Posta")]
    public string? EMail { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Kredi limiti negatif olamaz.")]
    [Display(Name = "Kredi Limiti")]
    public decimal KrediLimiti { get; set; }
}
