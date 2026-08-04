using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class SatisSiparisiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Müşteri seçilmelidir.")]
    [Display(Name = "Müşteri")]
    public int? CariId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Teslim Tarihi")]
    public DateTime? TeslimTarihi { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    // Dolu ise sipariş bu satış teklifinden oluşturulmuştur.
    public int? SatisTeklifiId { get; set; }
    public string? TeklifNoGosterim { get; set; }

    public List<SatisSiparisiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> MusteriListesi { get; set; } = [];
}

public class SatisSiparisiKalemFormViewModel
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
