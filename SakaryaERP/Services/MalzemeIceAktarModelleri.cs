namespace SakaryaERP.Services;

// Excel'den okunan ham (henüz doğrulanmamış) bir satır. Tüm alanlar kasıtlı olarak
// string — sayı/enum çözümleme ve doğrulama MalzemeService.TopluIceAktarAsync içinde yapılır.
public class MalzemeImportSatiri
{
    public int SatirNo { get; set; }
    public string? MalzemeKodu { get; set; }
    public string? Barkod { get; set; }
    public string? MalzemeAdi { get; set; }
    public string? Marka { get; set; }
    public string? Kalite { get; set; }
    public string? Tip { get; set; }
    public string? Birim { get; set; }
    public string? KategoriAdi { get; set; }
    public string? TeminTuru { get; set; }
    public string? StokTipi { get; set; }
    public string? AlisFiyati { get; set; }
    public string? SatisFiyati { get; set; }
    public string? KdvOrani { get; set; }
    public string? MinStokMiktari { get; set; }
    public string? MaxStokMiktari { get; set; }
    public string? RafNo { get; set; }
}

public class MalzemeIceAktarSonucu
{
    public int BasariliSayisi { get; set; }
    public List<string> Hatalar { get; set; } = [];
}
