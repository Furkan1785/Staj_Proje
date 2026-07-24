namespace SakaryaERP.ViewModels;

public class AnlikStokViewModel
{
    public int Id { get; set; }
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string? KategoriAdi { get; set; }
    public string Birim { get; set; } = "";
    public decimal Bakiye { get; set; }
    public decimal MinStokMiktari { get; set; }
    public decimal MaxStokMiktari { get; set; }
    public string DurumText { get; set; } = "";
    public string DurumSinifi { get; set; } = "";
}
