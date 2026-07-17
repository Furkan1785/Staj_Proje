namespace SakaryaERP.ViewModels;

public class MalzemeHareketFisiListItemViewModel
{
    public int Id { get; set; }
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string HareketTipiText { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public int KalemSayisi { get; set; }
    public string DurumText { get; set; } = "";
}
