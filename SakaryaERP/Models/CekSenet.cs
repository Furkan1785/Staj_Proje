namespace SakaryaERP.Models;

public class CekSenet : BaseEntity
{
    public BelgeTipi BelgeTipi { get; set; }
    public string BelgeNo { get; set; } = "";
    public int CariId { get; set; }
    public string? CiroBilgisi { get; set; }
    public DateTime VadeTarihi { get; set; }
    public decimal Tutar { get; set; }
    public string? BankaAdi { get; set; }
    public string? SubeAdi { get; set; }
    public CekSenetDurum Durum { get; set; } = CekSenetDurum.Portfoyde;

    public Cari Cari { get; set; } = null!;
}