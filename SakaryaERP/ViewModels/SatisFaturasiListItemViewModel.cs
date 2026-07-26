namespace SakaryaERP.ViewModels;

public class SatisFaturasiListItemViewModel
{
    public int Id { get; set; }
    public string FaturaNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime? VadeTarihi { get; set; }
    public string CariUnvan { get; set; } = "";
    public string SiparisNo { get; set; } = "-";
    public string IrsaliyeNo { get; set; } = "-";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
