namespace SakaryaERP.Models;

public class SatisTeklifi : BaseEntity
{
    public string TeklifNo { get; set; } = "";
    public int CariId { get; set; }
    public int? MusteriTalebiId { get; set; }
    public DateTime Tarih { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;
    public string? Aciklama { get; set; }

    // Teklifi oluşturan kullanıcının şubesi (otomatik doldurulur). Null ise eski kayıt
    // veya şubesiz (merkez) bir kullanıcı tarafından oluşturulmuştur.
    public int? SubeId { get; set; }
    public Sube? Sube { get; set; }

    public Cari Cari { get; set; } = null!;
    public MusteriTalebi? MusteriTalebi { get; set; }
    public ICollection<SatisTeklifiKalemi> Kalemler { get; set; } = new List<SatisTeklifiKalemi>();
    public ICollection<SatisSiparisi> SatisSiparisleri { get; set; } = new List<SatisSiparisi>();
}