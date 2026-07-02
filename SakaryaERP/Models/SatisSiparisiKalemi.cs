namespace SakaryaERP.Models;

public class SatisSiparisiKalemi : BaseEntity
{
    public int SatisSiparisiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public SatisSiparisi SatisSiparisi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}