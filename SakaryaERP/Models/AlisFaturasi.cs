namespace SakaryaERP.Models;

public class AlisFaturasi : BaseEntity
{
    public string FaturaNo { get; set; } = "";
    public int CariId { get; set; }
    public int? AlisSiparisiId { get; set; }
    public int? AlisIrsaliyesiId { get; set; }
    public DateTime Tarih { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    // Faturayı oluşturan kullanıcının şubesi (otomatik doldurulur). Null ise eski kayıt
    // veya şubesiz (merkez) bir kullanıcı tarafından oluşturulmuştur.
    public int? SubeId { get; set; }
    public Sube? Sube { get; set; }

    public Cari Cari { get; set; } = null!;
    public AlisSiparisi? AlisSiparisi { get; set; }
    public AlisIrsaliyesi? AlisIrsaliyesi { get; set; }
    public ICollection<AlisFaturasiKalemi> Kalemler { get; set; } = new List<AlisFaturasiKalemi>();
    public MuhasebeFisi? MuhasebeFisi { get; set; }
}