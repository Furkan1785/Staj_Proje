using Microsoft.AspNetCore.Mvc.Rendering;
using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class CekSenetDetayViewModel
{
    public int Id { get; set; }
    public BelgeTipi BelgeTipi { get; set; }
    public string BelgeTipiText { get; set; } = "";
    public string BelgeNo { get; set; } = "";
    public string CariUnvan { get; set; } = "";
    public string? CiroBilgisi { get; set; }
    public DateTime VadeTarihi { get; set; }
    public decimal Tutar { get; set; }
    public string? BankaAdi { get; set; }
    public string? SubeAdi { get; set; }
    public CekSenetDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public IEnumerable<SelectListItem> BankaHesabiListesi { get; set; } = [];
    public IEnumerable<SelectListItem> KasaHesabiListesi { get; set; } = [];
}
