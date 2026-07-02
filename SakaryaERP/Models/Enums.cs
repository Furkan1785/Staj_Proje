namespace SakaryaERP.Models;

public enum CariTipi { Musteri, Tedarikci, HerIkisi }

public enum FisTipi { Borc, Alacak, Mahsup }

public enum OdemeYontemi { Nakit, Havale, KrediKarti }

public enum BelgeTipi { Cek, Senet }

public enum CekSenetDurum { Portfolyde, Tahsilde, Ciro, Karsiliqsiz, TahsilEdildi }

public enum HesapTipi { Aktif, Pasif, Gelir, Gider, Ozkaynak }

public enum HareketTipi { Giris, Cikis, Transfer, Fire }

public enum TeminTuru { Alis, Uretim, AlisUretim }

public enum StokTipi { TicariMal, Hammadde, YariMamul, Mamul }

public enum BelgeDurum { Beklemede, Onaylandi, Iptal }

public enum TalepDurum { Yeni, Isleniyor, Tamamlandi, Iptal }