using System.ComponentModel.DataAnnotations;

namespace SakaryaERP.ViewModels;

public class KasaHesabiFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kasa adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Kasa adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Kasa Adı")]
    public string KasaAdi { get; set; } = "";

    [Required(ErrorMessage = "Para birimi zorunludur.")]
    [Display(Name = "Para Birimi")]
    public string ParaBirimi { get; set; } = "TRY";
}
