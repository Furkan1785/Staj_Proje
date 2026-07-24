namespace SakaryaERP.Models;

public class AlisSiparisi : BaseEntity
{
    public string SiparisNo { get; set; } = "";
    public int CariId { get; set; }
    public int SubeId { get; set; }
    public int? AlisTeklifiId { get; set; }
    public DateTime Tarih { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    public Cari Cari { get; set; } = null!;
    public Sube Sube { get; set; } = null!;
    public AlisTeklifi? AlisTeklifi { get; set; }
    public ICollection<AlisSiparisiKalemi> Kalemler { get; set; } = new List<AlisSiparisiKalemi>();
    public ICollection<AlisIrsaliyesi> AlisIrsaliyeleri { get; set; } = new List<AlisIrsaliyesi>();
    public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
}