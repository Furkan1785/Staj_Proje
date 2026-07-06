using System.ComponentModel.DataAnnotations;

namespace SakaryaERP.ViewModels;

public class SubeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Şube adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Şube Adı")]
    public string SubeAdi { get; set; } = "";

    [Display(Name = "Adres")]
    public string? Adres { get; set; }
}
