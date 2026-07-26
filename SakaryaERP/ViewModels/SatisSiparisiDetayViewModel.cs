using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class SatisSiparisiDetayViewModel
{
    public int Id { get; set; }
    public string SiparisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string? TeklifNo { get; set; }
    public string? Aciklama { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<SatisSiparisiKalemDetayViewModel> Kalemler { get; set; } = [];

    public decimal ToplamTutar => Kalemler.Sum(k => k.SatirToplami);
}

public class SatisSiparisiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }
    public decimal SevkEdilenMiktar { get; set; }
    public decimal KalanMiktar { get; set; }
    public decimal SevkYuzdesi { get; set; }

    public decimal SatirToplami => Math.Round(Miktar * BirimFiyat * (1 - Iskonto / 100) * (1 + KdvOrani / 100), 2);
}
