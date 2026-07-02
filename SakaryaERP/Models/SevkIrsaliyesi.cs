namespace SakaryaERP.Models;

public class SevkIrsaliyesi : BaseEntity
{
    public int SatisSiparisiId { get; set; }
    public int SubeId { get; set; }
    public DateTime Tarih { get; set; }
    public string? SevkAdresi { get; set; }
    public string? AracSofor { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;

    public SatisSiparisi SatisSiparisi { get; set; } = null!;
    public Sube Sube { get; set; } = null!;
    public ICollection<SevkIrsaliyesiKalemi> Kalemler { get; set; } = new List<SevkIrsaliyesiKalemi>();
    public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
}