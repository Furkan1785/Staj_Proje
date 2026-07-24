namespace SakaryaERP.Models;

public class AlisTeklifiKalemi : BaseEntity
{
    public int AlisTeklifiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public AlisTeklifi AlisTeklifi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}
