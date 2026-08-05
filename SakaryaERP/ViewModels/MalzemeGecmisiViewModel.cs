using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakaryaERP.ViewModels;

public class MalzemeGecmisiViewModel
{
    public int? MalzemeId { get; set; }
    public DateTime? Baslangic { get; set; }
    public DateTime? Bitis { get; set; }
    public int? SubeId { get; set; }
    public IEnumerable<SelectListItem> MalzemeListesi { get; set; } = [];
    public IEnumerable<SelectListItem> SubeListesi { get; set; } = [];

    public string? MalzemeAdi { get; set; }
    public string? Birim { get; set; }
    public decimal GuncelBakiye { get; set; }
    public List<MalzemeGecmisiSatiriViewModel> Satirlar { get; set; } = [];
    public decimal ToplamGiris { get; set; }
    public decimal ToplamCikis { get; set; }

    // Kümülatif bakiye (kardeks) sadece şube filtresi yokken anlamlıdır — bir şubeye
    // filtrelenince satırlar tüm hareketlerin bir alt kümesi olur ve GuncelBakiye (tüm
    // şubelerin toplamı) ile artık uyuşmaz. Bu durumda satırlarda KumulatifBakiye null kalır.
    public bool KumulatifBakiyeGosterilebilir => SubeId is null;
    public decimal DevirBakiye { get; set; }
}

public class MalzemeGecmisiSatiriViewModel
{
    public DateTime Tarih { get; set; }
    public string FisNo { get; set; } = "";
    public string HareketTipiText { get; set; } = "";
    public string SubeAdi { get; set; } = "";
    public decimal Giris { get; set; }
    public decimal Cikis { get; set; }
    public string? Aciklama { get; set; }
    public decimal? KumulatifBakiye { get; set; }

    // Tarih sadece gün çözünürlüğünde (saat bilgisi yok) — aynı güne düşen birden fazla
    // hareket olduğunda sıralamayı ve kümülatif bakiye hesabını deterministik yapmak için
    // ikincil sıralama anahtarı olarak belgenin gerçek oluşturulma zamanı (BaseEntity.CreatedAt)
    // kullanılıyor, aksi halde DB'den gelen satır sırası (garanti edilmeyen) her istekte
    // farklı bir sıraya ve tutarsız kümülatif bakiyeye yol açabilirdi.
    public DateTime OlusturulmaZamani { get; set; }
}
