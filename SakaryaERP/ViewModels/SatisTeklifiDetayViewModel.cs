using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class SatisTeklifiDetayViewModel
{
    public int Id { get; set; }
    public string TeklifNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = "";
    public string? TalepNo { get; set; }
    public string? Aciklama { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public bool SiparisOlusturulabilirMi { get; set; }
    public List<BelgeZinciriAdimi> Zincir { get; set; } = [];
    public List<SatisTeklifiKalemDetayViewModel> Kalemler { get; set; } = [];

    public decimal ToplamTutar => Kalemler.Sum(k => k.SatirToplami);
}

public class SatisTeklifiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public decimal SatirToplami => Math.Round(Miktar * BirimFiyat * (1 - Iskonto / 100) * (1 + KdvOrani / 100), 2);
}
