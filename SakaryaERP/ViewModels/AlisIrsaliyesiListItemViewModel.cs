namespace SakaryaERP.ViewModels;

public class AlisIrsaliyesiListItemViewModel
{
    public int Id { get; set; }
    public string IrsaliyeNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public string SiparisNo { get; set; } = "-";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
    public int KalemSayisi { get; set; }
}
