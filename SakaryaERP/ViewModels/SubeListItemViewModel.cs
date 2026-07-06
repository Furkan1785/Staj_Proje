namespace SakaryaERP.ViewModels;

public class SubeListItemViewModel
{
    public int Id { get; set; }
    public string SubeAdi { get; set; } = "";
    public string? Adres { get; set; }
    public bool IsDeleted { get; set; }
}
