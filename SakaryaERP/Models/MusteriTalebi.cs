namespace SakaryaERP.Models;

public class MusteriTalebi : BaseEntity
{
    public int CariId { get; set; }
    public string TalepNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string? Icerik { get; set; }
    public TalepDurum Durum { get; set; } = TalepDurum.Yeni;

    public Cari Cari { get; set; } = null!;
    public ICollection<SatisTeklifi> SatisTeklifleri { get; set; } = new List<SatisTeklifi>();
}