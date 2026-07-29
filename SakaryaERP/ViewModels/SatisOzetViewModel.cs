namespace SakaryaERP.ViewModels;

public class SatisOzetViewModel
{
    public decimal GenelToplam { get; set; }
    public List<MusteriToplamViewModel> MusteriToplamlari { get; set; } = [];
    public List<MalzemeToplamViewModel> MalzemeToplamlari { get; set; } = [];
    public List<AylikToplamViewModel> AylikToplamlar { get; set; } = [];
}

public class MusteriToplamViewModel
{
    public string CariUnvan { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}

public class MalzemeToplamViewModel
{
    public string MalzemeAdi { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
