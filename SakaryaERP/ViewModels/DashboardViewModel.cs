namespace SakaryaERP.ViewModels;

public class DashboardViewModel
{
    public decimal GenelSatisToplamiTRY { get; set; }
    public decimal GenelSatisToplamiUSD { get; set; }
    public decimal GenelSatinalmaToplamiTRY { get; set; }
    public decimal GenelSatinalmaToplamiUSD { get; set; }

    public int OncekiYil { get; set; }
    public int BuYil { get; set; }

    public List<KategoriToplamViewModel> KategoriToplamlari { get; set; } = [];
    public List<AylikTrendViewModel> AylikTrend { get; set; } = [];
    public List<MusteriToplamViewModel> EnCokSatisYapilanMusteriler { get; set; } = [];
    public List<MalzemeToplamViewModel> EnCokSatilanMalzemeler { get; set; } = [];
    public List<AnlikStokViewModel> KritikStokListesi { get; set; } = [];
}

public class KategoriToplamViewModel
{
    public string KategoriAdi { get; set; } = "";
    public decimal ToplamTutar { get; set; }
}

public class AylikTrendViewModel
{
    public int Ay { get; set; }
    public string AyAdi { get; set; } = "";
    public decimal OncekiYilToplam { get; set; }
    public decimal BuYilToplam { get; set; }
}
