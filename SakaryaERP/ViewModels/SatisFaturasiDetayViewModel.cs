using SakaryaERP.Models;

namespace SakaryaERP.ViewModels;

public class SatisFaturasiDetayViewModel
{
    public int Id { get; set; }
    public string FaturaNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime? VadeTarihi { get; set; }
    public string CariUnvan { get; set; } = "";
    public string? SiparisNo { get; set; }
    public string? IrsaliyeNo { get; set; }
    public string? Aciklama { get; set; }
    public BelgeDurum Durum { get; set; }
    public string DurumText { get; set; } = "";
    public List<SatisFaturasiKalemDetayViewModel> Kalemler { get; set; } = [];

    public decimal AraToplam => Kalemler.Sum(k => k.IskontoOncesiTutar);
    public decimal ToplamIskonto => Kalemler.Sum(k => k.IskontoTutari);
    public decimal ToplamKdv => Kalemler.Sum(k => k.KdvTutari);
    public decimal ToplamTutar => Kalemler.Sum(k => k.SatirToplami);
}

public class SatisFaturasiKalemDetayViewModel
{
    public string MalzemeKodu { get; set; } = "";
    public string MalzemeAdi { get; set; } = "";
    public string Birim { get; set; } = "";
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    public decimal IskontoOncesiTutar => Miktar * BirimFiyat;
    public decimal IskontoTutari => Math.Round(IskontoOncesiTutar * Iskonto / 100, 2);
    public decimal IskontoluTutar => IskontoOncesiTutar - IskontoTutari;
    public decimal KdvTutari => Math.Round(IskontoluTutar * KdvOrani / 100, 2);
    public decimal SatirToplami => Math.Round(IskontoluTutar * (1 + KdvOrani / 100), 2);
}
