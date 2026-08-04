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

    public Cari Cari { get; set; } = null!;
    public BankaHesabi? BankaHesabi { get; set; }
    public KasaHesabi? KasaHesabi { get; set; }
}