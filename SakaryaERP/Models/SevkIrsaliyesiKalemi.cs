namespace SakaryaERP.Models;

public class SevkIrsaliyesiKalemi : BaseEntity
{
    public int SevkIrsaliyesiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }

    public SevkIrsaliyesi SevkIrsaliyesi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}