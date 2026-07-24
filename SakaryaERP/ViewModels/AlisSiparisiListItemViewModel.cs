namespace SakaryaERP.ViewModels;

public class AlisSiparisiListItemViewModel
{
    public int Id { get; set; }
    public string SiparisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
    public string TeslimDurumu { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
