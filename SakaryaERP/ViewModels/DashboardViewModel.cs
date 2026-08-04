namespace SakaryaERP.ViewModels;

public enum DashboardGorunumTuru
{
    Satis,
    Satinalma
}

public enum DashboardAralik
{
    TumZamanlar,
    BuAy,
    BuYil
}

public class DashboardViewModel
{
    public decimal GenelSatisToplamiTRY { get; set; }
    public decimal GenelSatisToplamiUSD { get; set; }
    public decimal GenelSatinalmaToplamiTRY { get; set; }
    public decimal GenelSatinalmaToplamiUSD { get; set; }
    public decimal BrutKar { get; set; }

    public int OncekiYil { get; set; }
    public int BuYil { get; set; }

    public List<KategoriToplamViewModel> KategoriBazliSatis { get; set; } = [];
    public List<KategoriToplamViewModel> KategoriBazliAlis { get; set; } = [];
    public List<AylikTrendViewModel> AylikSatisTrendi { get; set; } = [];
    public List<AylikTrendViewModel> AylikAlisTrendi { get; set; } = [];
    public List<MusteriToplamViewModel> EnCokSatisYapilanMusteriler { get; set; } = [];
    public List<MusteriToplamViewModel> EnCokAlisYapilanTedarikciler { get; set; } = [];
    public List<MalzemeToplamViewModel> EnCokSatilanMalzemeler { get; set; } = [];
    public List<MalzemeToplamViewModel> EnCokAlinanMalzemeler { get; set; } = [];
    public List<AnlikStokViewModel> KritikStokListesi { get; set; } = [];

    // Filtre state'i — sayfa formu bu değerlerle yeniden çiziliyor
    public DashboardGorunumTuru SeciliTur { get; set; }
    public DashboardAralik SeciliAralik { get; set; }
    public List<int> SeciliKategoriIdler { get; set; } = [];
    public List<KategoriFiltreViewModel> KategoriFiltreListesi { get; set; } = [];
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

public class KategoriFiltreViewModel
{
    public int Id { get; set; }
    public string KategoriAdi { get; set; } = "";
    public int Seviye { get; set; }
}
