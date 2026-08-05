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

    // Belgeyi kaydeden kullanıcının şubesi (otomatik doldurulur) — SubeAdi'dan farklı, o çekin
    // üzerindeki BANKA şubesinin adıdır (serbest metin). Null ise eski kayıt veya şubesiz
    // (merkez) bir kullanıcı tarafından oluşturulmuştur.
    public int? SubeId { get; set; }
    public Sube? Sube { get; set; }

    public Cari Cari { get; set; } = null!;
}