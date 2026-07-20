namespace SakaryaERP.ViewModels;

public class KritikStokViewModel
{
    public int Id { get; set; }
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Bakiye { get; set; }
    public decimal MinStokMiktari { get; set; }
}
