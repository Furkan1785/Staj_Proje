namespace SakaryaERP.Models;

public class AlisIrsaliyesiKalemi : BaseEntity
{
    public int AlisIrsaliyesiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }

    public AlisIrsaliyesi AlisIrsaliyesi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}