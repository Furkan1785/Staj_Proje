using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class SevkIrsaliyesiDetayViewModel
{
    public int Id { get; set; }
    public string IrsaliyeNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SiparisNo { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public string? SevkAdresi { get; set; }
    public string? AracSofor { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<BelgeZinciriAdimi> Zincir { get; set; } = [];
    public List<SevkIrsaliyesiKalemDetayViewModel> Kalemler { get; set; } = [];
}

public class SevkIrsaliyesiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
}
