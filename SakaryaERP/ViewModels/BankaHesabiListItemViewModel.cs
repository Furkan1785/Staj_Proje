namespace SakaryaERP.ViewModels;

public class BankaHesabiListItemViewModel
{
    public int Id { get; set; }
    public string HesapAdi { get; set; } = "";
    public string BankaAdi { get; set; } = "";
    public string? IBAN { get; set; }
    public decimal Bakiye { get; set; }
    public string ParaBirimi { get; set; } = "";
    public bool IsDeleted { get; set; }
}
