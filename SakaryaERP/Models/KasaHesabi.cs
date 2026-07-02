namespace SakaryaERP.Models;

public class KasaHesabi : BaseEntity
{
    public string KasaAdi { get; set; } = "";
    public decimal Bakiye { get; set; }
    public string ParaBirimi { get; set; } = "TRY";

    public ICollection<CariFisi> CariFisleri { get; set; } = new List<CariFisi>();
}