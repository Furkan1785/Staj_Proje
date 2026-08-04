namespace SakaryaERP.Models;

public class CariFisi : BaseEntity
{
    public string FisNo { get; set; } = "";
    public int CariId { get; set; }
    public DateTime Tarih { get; set; }
    public FisTipi FisTipi { get; set; }
    public decimal Tutar { get; set; }
    public OdemeYontemi OdemeYontemi { get; set; }
    public int? BankaHesabiId { get; set; }
    public int? KasaHesabiId { get; set; }
    public string? Aciklama { get; set; }

    // Fatura onayı/çek-senet tahsilatı gibi bir belgeden otomatik oluşmuşsa true;
    // bu tür fişler doğrudan iptal edilemez (kaynak belgeyle bağlantısı koparmasın diye).
    public bool OtomatikOlusturuldu { get; set; }

    // Otomatik oluşmuşsa hangi belgeden geldiği (kaynak belge iptal edilince bu fişi bulup
    // geri almak için) — aynı anda en fazla biri dolu olur.
    public int? AlisFaturasiId { get; set; }
    public int? SatisFaturasiId { get; set; }
    public int? CekSenetId { get; set; }

    public Cari Cari { get; set; } = null!;
    public BankaHesabi? BankaHesabi { get; set; }
    public KasaHesabi? KasaHesabi { get; set; }
    public AlisFaturasi? AlisFaturasi { get; set; }
    public SatisFaturasi? SatisFaturasi { get; set; }
    public CekSenet? CekSenet { get; set; }
}