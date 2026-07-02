namespace SakaryaERP.Models;

public class MalzemeKategori : BaseEntity
{
    public string KategoriAdi { get; set; } = "";
    public int? ParentId { get; set; }

    public MalzemeKategori? Parent { get; set; }
    public ICollection<MalzemeKategori> AltKategoriler { get; set; } = new List<MalzemeKategori>();
    public ICollection<Malzeme> Malzemeler { get; set; } = new List<Malzeme>();
}