using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class MusteriTalebiDetayViewModel
{
    public int Id { get; set; }
    public string TalepNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = "";
    public string? Icerik { get; set; }
    public TalepDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<MusteriTalebiTeklifOzetViewModel> Teklifler { get; set; } = [];
}

public class MusteriTalebiTeklifOzetViewModel
{
    public int Id { get; set; }
    public string TeklifNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string DurumText { get; set; } = "";
}
