namespace SakaryaERP.Models;

public class MuhasebeFisi : BaseEntity
{
    public string FisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int? SatisFaturasiId { get; set; }
    public int? AlisFaturasiId { get; set; }

    public SatisFaturasi? SatisFaturasi { get; set; }
    public AlisFaturasi? AlisFaturasi { get; set; }
    public ICollection<MuhasebeFisiKalemi> Kalemler { get; set; } = new List<MuhasebeFisiKalemi>();
}