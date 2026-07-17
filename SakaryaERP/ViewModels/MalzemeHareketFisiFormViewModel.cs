using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class MalzemeHareketFisiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Display(Name = "Hareket Tipi")]
    public HareketTipi HareketTipi { get; set; }

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? SubeId { get; set; }

    public List<MalzemeHareketFisiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> SubeListesi { get; set; } = [];
}

public class MalzemeHareketFisiKalemFormViewModel
{
    [Required(ErrorMessage = "Malzeme seçilmelidir.")]
    public int? MalzemeId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
    public decimal Miktar { get; set; }

    public string? Aciklama { get; set; }
}
