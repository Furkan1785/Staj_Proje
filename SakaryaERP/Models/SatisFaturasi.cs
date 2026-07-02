namespace SakaryaERP.Models;

public class SatisFaturasi : BaseEntity
{
    public int CariId { get; set; }
    public int? SevkIrsaliyesiId { get; set; }
    public int? SatisSiparisiId { get; set; }
    public DateTime Tarih { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    public Cari Cari { get; set; } = null!;
    public SevkIrsaliyesi? SevkIrsaliyesi { get; set; }
    public SatisSiparisi? SatisSiparisi { get; set; }
    public ICollection<SatisFaturasiKalemi> Kalemler { get; set; } = new List<SatisFaturasiKalemi>();
    public MuhasebeFisi? MuhasebeFisi { get; set; }
}