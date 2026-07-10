namespace SakaryaERP.ViewModels;

public class CekSenetListItemViewModel
{
    public int Id { get; set; }
    public string BelgeTipiText { get; set; } = "";
    public string BelgeNo { get; set; } = "";
    public string CariUnvan { get; set; } = "";
    public DateTime VadeTarihi { get; set; }
    public decimal Tutar { get; set; }
    public string? BankaAdi { get; set; }
    public string? SubeAdi { get; set; }
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
}
