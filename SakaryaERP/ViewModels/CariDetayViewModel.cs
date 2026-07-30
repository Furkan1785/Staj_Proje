namespace SakaryaERP.ViewModels;

public class CariDetayViewModel
{
    public int Id { get; set; }
    public string CariKodu { get; set; } = "";
    public string Unvan { get; set; } = "";
    public string CariTipiText { get; set; } = "";
    public string? VergiNo { get; set; }
    public string? Adres { get; set; }
    public string? Telefon { get; set; }
    public string? EMail { get; set; }
    public decimal Bakiye { get; set; }
    public decimal KrediLimiti { get; set; }
    public bool IsDeleted { get; set; }
    public List<CariEkstreSatiriViewModel> SonHareketler { get; set; } = [];
}
