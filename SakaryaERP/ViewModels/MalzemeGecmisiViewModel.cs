using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class MalzemeGecmisiViewModel
{
    public int? MalzemeId { get; set; }
    public DateTime? Baslangic { get; set; }
    public DateTime? Bitis { get; set; }
    public IEnumerable<SelectListItem> MalzemeListesi { get; set; } = [];

    public string? MalzemeAdi { get; set; }
    public string? Birim { get; set; }
    public decimal GuncelBakiye { get; set; }
    public List<MalzemeGecmisiSatiriViewModel> Satirlar { get; set; } = [];
    public decimal ToplamGiris { get; set; }
    public decimal ToplamCikis { get; set; }
}

public class MalzemeGecmisiSatiriViewModel
{
    public DateTime Tarih { get; set; }
    public string FisNo { get; set; } = "";
    public string HareketTipiText { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public decimal Giris { get; set; }
    public decimal Cikis { get; set; }
    public string? Aciklama { get; set; }
}
