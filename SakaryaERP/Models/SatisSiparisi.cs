namespace SakaryaERP.Models;

public class SatisSiparisi : BaseEntity
{
    public string SiparisNo { get; set; } = "";
    public int CariId { get; set; }
    public int? SatisTeklifiId { get; set; }
    public DateTime Tarih { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    public Cari Cari { get; set; } = null!;
    public SatisTeklifi? SatisTeklifi { get; set; }
    public ICollection<SatisSiparisiKalemi> Kalemler { get; set; } = new List<SatisSiparisiKalemi>();
    public ICollection<SevkIrsaliyesi> SevkIrsaliyeleri { get; set; } = new List<SevkIrsaliyesi>();
    public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
}