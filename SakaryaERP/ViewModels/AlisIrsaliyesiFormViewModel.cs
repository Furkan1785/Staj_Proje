using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class AlisIrsaliyesiFormViewModel
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

    // Dolu ise irsaliye bu alış siparişinden oluşturulmuştur.
    public int? AlisSiparisiId { get; set; }
    public string? SiparisNoGosterim { get; set; }

    public List<AlisIrsaliyesiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> TedarikciListesi { get; set; } = [];
    public IEnumerable<SelectListItem> SubeListesi { get; set; } = [];
}

public class AlisIrsaliyesiKalemFormViewModel
{
    [Required(ErrorMessage = "Malzeme seçilmelidir.")]
    public int? MalzemeId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
    public decimal Miktar { get; set; }
}
