using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class MalzemeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Malzeme kodu zorunludur.")]
    [StringLength(30, ErrorMessage = "Malzeme kodu en fazla 30 karakter olabilir.")]
    [Display(Name = "Malzeme Kodu")]
    public string MalzemeKodu { get; set; } = "";

    [StringLength(50, ErrorMessage = "Barkod en fazla 50 karakter olabilir.")]
    [Display(Name = "Barkod")]
    public string? Barkod { get; set; }

    [Required(ErrorMessage = "Malzeme adı zorunludur.")]
    [StringLength(200, ErrorMessage = "Malzeme adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Malzeme Adı")]
    public string MalzemeAdi { get; set; } = "";

    [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olabilir.")]
    [Display(Name = "Marka")]
    public string? Marka { get; set; }

    [StringLength(50, ErrorMessage = "Kalite en fazla 50 karakter olabilir.")]
    [Display(Name = "Kalite")]
    public string? Kalite { get; set; }

    [StringLength(50, ErrorMessage = "Tip en fazla 50 karakter olabilir.")]
    [Display(Name = "Tip")]
    public string? Tip { get; set; }

    [Required(ErrorMessage = "Birim zorunludur.")]
    [StringLength(20, ErrorMessage = "Birim en fazla 20 karakter olabilir.")]
    [Display(Name = "Birim")]
    public string Birim { get; set; } = "";

    [Display(Name = "Kategori")]
    public int? KategoriId { get; set; }

    [Display(Name = "Temin Türü")]
    public TeminTuru TeminTuru { get; set; }

    [Display(Name = "Stok Tipi")]
    public StokTipi StokTipi { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Alış fiyatı negatif olamaz.")]
    [Display(Name = "Alış Fiyatı")]
    public decimal AlisFiyati { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı negatif olamaz.")]
    [Display(Name = "Satış Fiyatı")]
    public decimal SatisFiyati { get; set; }

    [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
    [Display(Name = "KDV Oranı")]
    public decimal KdvOrani { get; set; } = 20;

    [Range(0, double.MaxValue, ErrorMessage = "Min stok negatif olamaz.")]
    [Display(Name = "Min Stok Miktarı")]
    public decimal MinStokMiktari { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max stok negatif olamaz.")]
    [Display(Name = "Max Stok Miktarı")]
    public decimal MaxStokMiktari { get; set; }

    [StringLength(20, ErrorMessage = "Raf no en fazla 20 karakter olabilir.")]
    [Display(Name = "Raf No")]
    public string? RafNo { get; set; }

    public IEnumerable<SelectListItem> KategoriListesi { get; set; } = [];
}
