namespace SakaryaERP.ViewModels;

public class SatisTeklifiListItemViewModel
{
    public int Id { get; set; }
    public string TeklifNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string CariUnvan { get; set; } = "";
    public string TalepNo { get; set; } = "-";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
