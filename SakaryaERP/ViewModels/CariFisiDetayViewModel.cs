using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class CariFisiDetayViewModel
{
    public int Id { get; set; }
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = "";
    public FisTipi FisTipi { get; set; }
    public string FisTipiText { get; set; } = "";
    public decimal Tutar { get; set; }
    public string OdemeYontemiText { get; set; } = "";
    public string? HesapAdi { get; set; }
    public string? Aciklama { get; set; }
    public bool IptalEdildi { get; set; }
}
