namespace SakaryaERP.Models;

public class AlisSiparisiKalemi : BaseEntity
{
    public int AlisSiparisiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }

    public AlisSiparisi AlisSiparisi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}