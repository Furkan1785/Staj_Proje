using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class CekSenetIndexViewModel
{
    public IEnumerable<SelectListItem> BankaHesabiListesi { get; set; } = [];
    public IEnumerable<SelectListItem> KasaHesabiListesi { get; set; } = [];
}
