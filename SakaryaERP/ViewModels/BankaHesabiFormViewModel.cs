using System.ComponentModel.DataAnnotations;

namespace SakaryaERP.ViewModels;

public class BankaHesabiFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hesap adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Hesap adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Hesap Adı")]
    public string HesapAdi { get; set; } = "";

    [Required(ErrorMessage = "Banka adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Banka Adı")]
    public string BankaAdi { get; set; } = "";

    [StringLength(34, ErrorMessage = "IBAN en fazla 34 karakter olabilir.")]
    [Display(Name = "IBAN")]
    public string? IBAN { get; set; }

    [Required(ErrorMessage = "Para birimi zorunludur.")]
    [Display(Name = "Para Birimi")]
    public string ParaBirimi { get; set; } = "TRY";
}
