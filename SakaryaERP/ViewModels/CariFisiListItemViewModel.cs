namespace SakaryaERP.ViewModels;

public class CariFisiListItemViewModel
{
    public int Id { get; set; }
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string CariUnvan { get; set; } = "";
    public string FisTipiText { get; set; } = "";
    public decimal Tutar { get; set; }
    public string OdemeYontemiText { get; set; } = "";
    public string? HesapAdi { get; set; }
    public string? Aciklama { get; set; }
}
