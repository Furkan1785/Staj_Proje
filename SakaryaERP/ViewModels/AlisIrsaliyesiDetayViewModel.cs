using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class AlisIrsaliyesiDetayViewModel
{
    public int Id { get; set; }
    public string IrsaliyeNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public string? SiparisNo { get; set; }
    public string? Aciklama { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<AlisIrsaliyesiKalemDetayViewModel> Kalemler { get; set; } = [];
}

public class AlisIrsaliyesiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
}
