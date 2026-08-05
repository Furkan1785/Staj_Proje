namespace SakaryaERP.ViewModels;

public class BelgeTakipViewModel
{
    public int GunSayisi { get; set; }

    public List<BelgeTakipSatiriViewModel> GecikenTeklifler { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> YaklasanTeklifler { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> GecikenSatisSiparisleri { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> YaklasanSatisSiparisleri { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> GecikenAlisSiparisleri { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> YaklasanAlisSiparisleri { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> GecikenSatisFaturalari { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> YaklasanSatisFaturalari { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> GecikenAlisFaturalari { get; set; } = [];
    public List<BelgeTakipSatiriViewModel> YaklasanAlisFaturalari { get; set; } = [];
}

public class BelgeTakipSatiriViewModel
{
    public int Id { get; set; }
    public string BelgeNo { get; set; } = "";
    public string CariUnvan { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int GunFarki { get; set; }
}
