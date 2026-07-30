using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class MalzemeHareketFisiDetayViewModel
{
    public int Id { get; set; }
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public HareketTipi HareketTipi { get; set; }
    public string HareketTipiText { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<MalzemeHareketFisiKalemDetayViewModel> Kalemler { get; set; } = [];
}

public class MalzemeHareketFisiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
    public string? Aciklama { get; set; }
}
