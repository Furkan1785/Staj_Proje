namespace SakaryaERP.Models;

public class HesapPlani : BaseEntity
{
    public string HesapKodu { get; set; } = "";
    public string HesapAdi { get; set; } = "";
    public HesapTipi HesapTipi { get; set; }
    public int? ParentId { get; set; }

    public HesapPlani? Parent { get; set; }
    public ICollection<HesapPlani> AltHesaplar { get; set; } = new List<HesapPlani>();
    public ICollection<MuhasebeFisiKalemi> MuhasebeFisiKalemleri { get; set; } = new List<MuhasebeFisiKalemi>();
}