namespace SakaryaERP.Models;

public class MalzemeHareketFisi : BaseEntity
{
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public HareketTipi HareketTipi { get; set; }
    public int SubeId { get; set; }
    public BelgeDurum Durum { get; set; } = BelgeDurum.Beklemede;

    public Sube Sube { get; set; } = null!;
    public ICollection<MalzemeHareketFisiKalemi> Kalemler { get; set; } = new List<MalzemeHareketFisiKalemi>();
}