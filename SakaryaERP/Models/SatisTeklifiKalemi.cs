namespace SakaryaERP.Models;

public class SatisTeklifiKalemi : BaseEntity
{
    public int SatisTeklifiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public SatisTeklifi SatisTeklifi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}