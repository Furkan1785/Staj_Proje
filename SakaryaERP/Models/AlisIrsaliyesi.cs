namespace SakaryaERP.Models;

public class AlisIrsaliyesi : BaseEntity
{
    public int? AlisSiparisiId { get; set; }
    public int CariId { get; set; }
    public int SubeId { get; set; }
    public DateTime Tarih { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    public AlisSiparisi? AlisSiparisi { get; set; }
    public Cari Cari { get; set; } = null!;
    public Sube Sube { get; set; } = null!;
    public ICollection<AlisIrsaliyesiKalemi> Kalemler { get; set; } = new List<AlisIrsaliyesiKalemi>();
    public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
}