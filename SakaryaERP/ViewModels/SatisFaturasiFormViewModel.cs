using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class SatisFaturasiFormViewModel
{
    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Vade Tarihi")]
    public DateTime? VadeTarihi { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Müşteri seçilmelidir.")]
    [Display(Name = "Müşteri")]
    public int? CariId { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    // Dolu ise fatura bu sevk irsaliyesinden/satış siparişinden oluşturulmuştur.
    public int? SevkIrsaliyesiId { get; set; }
    public int? SatisSiparisiId { get; set; }
    public string? IrsaliyeNoGosterim { get; set; }
    public string? SiparisNoGosterim { get; set; }

    public List<SatisFaturasiKalemFormViewModel> Kalemler { get; set; } = [];

    public IEnumerable<SelectListItem> MusteriListesi { get; set; } = [];
}

public class SatisFaturasiKalemFormViewModel
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
