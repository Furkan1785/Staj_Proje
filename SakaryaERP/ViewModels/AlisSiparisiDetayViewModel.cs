using SakaryaERP.Helpers;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class AlisSiparisiDetayViewModel
{
    public int Id { get; set; }
    public string SiparisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime? TeslimTarihi { get; set; }
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public string? Aciklama { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<BelgeZinciriAdimi> Zincir { get; set; } = [];
    public List<AlisSiparisiKalemDetayViewModel> Kalemler { get; set; } = [];

    public decimal ToplamTutar => Kalemler.Sum(k => k.SatirToplami);
}

public class AlisSiparisiKalemDetayViewModel : IFiyatliKalem
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }
    public decimal TeslimAlinanMiktar { get; set; }
    public decimal KalanMiktar { get; set; }
    public decimal TeslimYuzdesi { get; set; }

    public decimal SatirToplami => FinansHesaplama.SatirToplami(this);
}
