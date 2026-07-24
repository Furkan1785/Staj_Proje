namespace SakaryaERP.ViewModels;

public class MusteriTalebiListItemViewModel
{
    public int Id { get; set; }
    public string TalepNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string Icerik { get; set; } = "";
    public string Durum { get; set; } = "";
    public string DurumText { get; set; } = "";
}
