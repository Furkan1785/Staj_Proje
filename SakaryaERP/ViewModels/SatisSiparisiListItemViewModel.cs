namespace SakaryaERP.ViewModels;

public class SatisSiparisiListItemViewModel
{
    public int Id { get; set; }
    public string SiparisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string TeklifNo { get; set; } = "-";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
    public string SevkDurumu { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
