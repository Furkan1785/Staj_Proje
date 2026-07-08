using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class CariFisiFormViewModel
{
    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int CariId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Display(Name = "Fiş Tipi")]
    public FisTipi FisTipi { get; set; }

    [Required(ErrorMessage = "Tutar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
    [Display(Name = "Tutar")]
    public decimal Tutar { get; set; }

    [Display(Name = "Ödeme Yöntemi")]
    public OdemeYontemi OdemeYontemi { get; set; }

    [Display(Name = "Banka Hesabı")]
    public int? BankaHesabiId { get; set; }

    [Display(Name = "Kasa Hesabı")]
    public int? KasaHesabiId { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public IEnumerable<SelectListItem> CariListesi { get; set; } = [];
    public IEnumerable<SelectListItem> BankaHesabiListesi { get; set; } = [];
    public IEnumerable<SelectListItem> KasaHesabiListesi { get; set; } = [];
}
