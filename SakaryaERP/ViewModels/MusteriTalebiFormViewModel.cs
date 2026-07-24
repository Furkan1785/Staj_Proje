using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class MusteriTalebiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Müşteri seçilmelidir.")]
    [Display(Name = "Müşteri")]
    public int? CariId { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    [StringLength(1000, ErrorMessage = "İçerik en fazla 1000 karakter olabilir.")]
    [Display(Name = "İçerik")]
    public string Icerik { get; set; } = "";

    public IEnumerable<SelectListItem> MusteriListesi { get; set; } = [];
}
