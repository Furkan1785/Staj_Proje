using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class SevkIrsaliyesiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Satış siparişi seçilmelidir.")]
    public int SatisSiparisiId { get; set; }

    public string SiparisNoGosterim { get; set; } = "";
    public string CariUnvanGosterim { get; set; } = "";

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? SubeId { get; set; }

    [StringLength(300, ErrorMessage = "Sevk adresi en fazla 300 karakter olabilir.")]
    [Display(Name = "Sevk Adresi")]
    public string? SevkAdresi { get; set; }

    [StringLength(200, ErrorMessage = "Araç/Şoför bilgisi en fazla 200 karakter olabilir.")]
    [Display(Name = "Araç / Şoför")]
    public string? AracSofor { get; set; }

    public List<SevkIrsaliyesiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> SubeListesi { get; set; } = [];
}

public class SevkIrsaliyesiKalemFormViewModel
{
    [Required(ErrorMessage = "Malzeme seçilmelidir.")]
    public int? MalzemeId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
    public decimal Miktar { get; set; }
}
