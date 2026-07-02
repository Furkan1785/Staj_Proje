namespace SakaryaERP.Models;

public class BankaHesabi : BaseEntity
{
    public string HesapAdi { get; set; } = "";
    public string BankaAdi { get; set; } = "";
    public string? IBAN { get; set; }
    public decimal Bakiye { get; set; }
    public string ParaBirimi { get; set; } = "TRY";

    public ICollection<CariFisi> CariFisleri { get; set; } = new List<CariFisi>();
}