using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class CekSenetFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Belge Tipi")]
    public BelgeTipi BelgeTipi { get; set; }

    [Required(ErrorMessage = "Belge No zorunludur.")]
    [StringLength(50, ErrorMessage = "Belge No en fazla 50 karakter olabilir.")]
    [Display(Name = "Belge No")]
    public string BelgeNo { get; set; } = "";

    [Required(ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int CariId { get; set; }

    [StringLength(250, ErrorMessage = "Ciro bilgisi en fazla 250 karakter olabilir.")]
    [Display(Name = "Ciro Bilgisi")]
    public string? CiroBilgisi { get; set; }

    [Required(ErrorMessage = "Vade tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Vade Tarihi")]
    public DateTime VadeTarihi { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Tutar zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
    [Display(Name = "Tutar")]
    public decimal Tutar { get; set; }

    [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Banka Adı")]
    public string? BankaAdi { get; set; }

    [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Şube Adı")]
    public string? SubeAdi { get; set; }

    public IEnumerable<SelectListItem> CariListesi { get; set; } = [];
}
