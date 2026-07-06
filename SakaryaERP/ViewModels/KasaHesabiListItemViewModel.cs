namespace SakaryaERP.ViewModels;

public class KasaHesabiListItemViewModel
{
    public int Id { get; set; }
    public string KasaAdi { get; set; } = "";
    public decimal Bakiye { get; set; }
    public string ParaBirimi { get; set; } = "";
    public bool IsDeleted { get; set; }
}
