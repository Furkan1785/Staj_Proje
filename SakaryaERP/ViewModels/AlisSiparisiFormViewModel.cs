using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class AlisSiparisiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Tedarikçi seçilmelidir.")]
    [Display(Name = "Tedarikçi")]
    public int? CariId { get; set; }

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? SubeId { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public List<AlisSiparisiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> TedarikciListesi { get; set; } = [];
    public IEnumerable<SelectListItem> SubeListesi { get; set; } = [];
}

public class AlisSiparisiKalemFormViewModel
{
    [Required(ErrorMessage = "Malzeme seçilmelidir.")]
    public int? MalzemeId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
    public decimal Miktar { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
    public decimal BirimFiyat { get; set; }

    [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
    public decimal KdvOrani { get; set; } = 20;

    [Range(0, 100, ErrorMessage = "İskonto oranı 0-100 arasında olmalıdır.")]
    public decimal Iskonto { get; set; }
}
