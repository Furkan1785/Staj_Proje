namespace SakaryaERP.ViewModels;

public class HesapPlaniListItemViewModel
{
    public int Id { get; set; }
    public string HesapKodu { get; set; } = "";
    public string HesapAdi { get; set; } = "";
    public string HesapTipi { get; set; } = "";
    public int Seviye { get; set; }
}
