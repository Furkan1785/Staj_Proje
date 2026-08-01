namespace SakaryaERP.ViewModels;

public class MalzemeDetayViewModel
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
    public List<MalzemeGecmisiSatiriViewModel> SonHareketler { get; set; } = [];
    public List<MalzemeFaturaSatiriViewModel> SonSatislar { get; set; } = [];
    public List<MalzemeFaturaSatiriViewModel> SonAlislar { get; set; } = [];
}

// Malzeme.Bakiye, satış/alış faturası onayında MalzemeHareketFisi oluşturulmadan
// doğrudan güncelleniyor (bkz. SatisFaturasiService/AlisFaturasiService) — yani
// "Hareket Geçmişi" sekmesi (sadece MalzemeHareketFisiKalemi'ne bakıyor) bu
// hareketleri göstermiyor. Bu kör noktayı kapatmak için ayrı bir sekme.
public class MalzemeFaturaSatiriViewModel
{
    public int FaturaId { get; set; }
    public DateTime Tarih { get; set; }
    public string FaturaNo { get; set; } = "";
    public string CariUnvan { get; set; } = "";
    public decimal Miktar { get; set; }
}
