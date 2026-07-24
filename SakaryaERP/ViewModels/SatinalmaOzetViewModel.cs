namespace SakaryaERP.ViewModels;

public class SatinalmaOzetViewModel
{
    public decimal GenelToplam { get; set; }
    public List<TedarikciToplamViewModel> TedarikciToplamlari { get; set; } = [];
    public List<AylikToplamViewModel> AylikToplamlar { get; set; } = [];
}

public class TedarikciToplamViewModel
{
    public string CariUnvan { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}

public class AylikToplamViewModel
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public string Etiket { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}
