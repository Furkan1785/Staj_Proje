namespace SakaryaERP.Models;

public class MuhasebeFisiKalemi : BaseEntity
{
    public int MuhasebeFisiId { get; set; }
    public int HesapPlaniId { get; set; }
    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }
    public string? Aciklama { get; set; }

    public MuhasebeFisi MuhasebeFisi { get; set; } = null!;
    public HesapPlani HesapPlani { get; set; } = null!;
}