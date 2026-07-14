namespace SakaryaERP.ViewModels;

public class MalzemeListItemViewModel
{
    public int Id { get; set; }
    public string MalzemeKodu { get; set; } = "";
    public string? Barkod { get; set; }
    public string MalzemeAdi { get; set; } = "";
    public string? Marka { get; set; }
    public string? Kalite { get; set; }
    public string? Tip { get; set; }
    public string Birim { get; set; } = "";
    public string? KategoriAdi { get; set; }
    public string TeminTuruText { get; set; } = "";
    public string StokTipiText { get; set; } = "";
    public decimal AlisFiyati { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal MinStokMiktari { get; set; }
    public decimal MaxStokMiktari { get; set; }
    public string? RafNo { get; set; }
    public decimal Bakiye { get; set; }
}
