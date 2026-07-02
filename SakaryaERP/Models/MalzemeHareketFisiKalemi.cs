namespace SakaryaERP.Models;

public class MalzemeHareketFisiKalemi : BaseEntity
{
    public int MalzemeHareketFisiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public string? Aciklama { get; set; }

    public MalzemeHareketFisi MalzemeHareketFisi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}