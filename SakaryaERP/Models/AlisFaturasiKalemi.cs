namespace SakaryaERP.Models;

public class AlisFaturasiKalemi : BaseEntity, IFiyatliKalem
{
    public int AlisFaturasiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public AlisFaturasi AlisFaturasi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}