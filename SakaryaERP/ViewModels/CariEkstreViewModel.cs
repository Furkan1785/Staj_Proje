using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class CariEkstreViewModel
{
    public int? CariId { get; set; }
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public IEnumerable<SelectListItem> CariListesi { get; set; } = [];

    public string? CariUnvan { get; set; }
    public decimal DevirBakiye { get; set; }
    public List<CariEkstreSatiriViewModel> Satirlar { get; set; } = [];
    public decimal SonBakiye { get; set; }
}

public class CariEkstreSatiriViewModel
{
    public DateTime Tarih { get; set; }
    public string FisNo { get; set; } = "";
    public string FisTipiText { get; set; } = "";
    public string? Aciklama { get; set; }
    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }
    public decimal KumulatifBakiye { get; set; }
}
