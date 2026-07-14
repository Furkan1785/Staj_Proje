using System.ComponentModel.DataAnnotations;

namespace SakaryaERP.Models;

public enum CariTipi
{
    [Display(Name = "Müşteri")]
    Musteri,
    [Display(Name = "Tedarikçi")]
    Tedarikci,
    [Display(Name = "Her İkisi")]
    HerIkisi
}

public enum FisTipi
{
    [Display(Name = "Borç")]
    Borc,
    [Display(Name = "Alacak")]
    Alacak,
    [Display(Name = "Mahsup")]
    Mahsup
}

public enum OdemeYontemi
{
    [Display(Name = "Nakit")]
    Nakit,
    [Display(Name = "Havale")]
    Havale,
    [Display(Name = "Kredi Kartı")]
    KrediKarti
}

public enum BelgeTipi
{
    [Display(Name = "Çek")]
    Cek,
    [Display(Name = "Senet")]
    Senet
}

public enum CekSenetDurum
{
    [Display(Name = "Portföyde")]
    Portfoyde,
    [Display(Name = "Tahsilde")]
    Tahsilde,
    [Display(Name = "Ciro")]
    Ciro,
    [Display(Name = "Karşılıksız")]
    Karsiliksiz,
    [Display(Name = "Tahsil Edildi")]
    TahsilEdildi
}

public enum HesapTipi { Aktif, Pasif, Gelir, Gider, Ozkaynak }

public enum HareketTipi { Giris, Cikis, Transfer, Fire }

public enum TeminTuru
{
    [Display(Name = "Alış")]
    Alis,
    [Display(Name = "Üretim")]
    Uretim,
    [Display(Name = "Alış+Üretim")]
    AlisUretim
}

public enum StokTipi
{
    [Display(Name = "Ticari Mal")]
    TicariMal,
    [Display(Name = "Hammadde")]
    Hammadde,
    [Display(Name = "Yarı Mamul")]
    YariMamul,
    [Display(Name = "Mamul")]
    Mamul
}

public enum BelgeDurum { Beklemede, Onaylandi, Iptal }

public enum TalepDurum { Yeni, Isleniyor, Tamamlandi, Iptal }