namespace SakaryaERP.Models;

public class AlisTeklifi : BaseEntity
{
    public int CariId { get; set; }
    public int? AlisTalebiId { get; set; }
    public DateTime Tarih { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    public Cari Cari { get; set; } = null!;
    public AlisTalebi? AlisTalebi { get; set; }
    public ICollection<AlisTeklifiKalemi> Kalemler { get; set; } = new List<AlisTeklifiKalemi>();
    public ICollection<AlisSiparisi> AlisSiparisleri { get; set; } = new List<AlisSiparisi>();
}
