namespace SakaryaERP.Models;

public class Malzeme : BaseEntity
{
    public string MalzemeKodu { get; set; } = "";
    public string? Barkod { get; set; }
    public string MalzemeAdi { get; set; } = "";
    public string? Marka { get; set; }
    public string? Kalite { get; set; }
    public string? Tip { get; set; }
    public string Birim { get; set; } = "";
    public TeminTuru TeminTuru { get; set; }
    public StokTipi StokTipi { get; set; }
    public decimal AlisFiyati { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal MinStokMiktari { get; set; }
    public decimal MaxStokMiktari { get; set; }
    public string? RafNo { get; set; }
    public decimal Bakiye { get; set; }
    public int? KategoriId { get; set; }

    public MalzemeKategori? Kategori { get; set; }
    public ICollection<MalzemeHareketFisiKalemi> HareketKalemleri { get; set; } = new List<MalzemeHareketFisiKalemi>();
}