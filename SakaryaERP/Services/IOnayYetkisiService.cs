namespace SakaryaERP.Services;

public interface IOnayYetkisiService
{
    // Tutar eşiğinin üzerindeki bir belgeyi sadece Admin onaylayabilir; eşiğin
    // altındaki belgeler için mevcut rol bazlı [Authorize] kısıtları (Controller
    // düzeyinde) zaten yeterli, bu yüzden burada ek bir kontrol yapılmıyor.
    void YuksekTutarKontrolEt(decimal tutar, string belgeTuru);

    // Görevler ayrılığı: bir belgeyi oluşturan kişi (Admin hariç) kendi belgesini
    // onaylayamaz. olusturanKullanici null ise (eski kayıt veya sistem/seed
    // tarafından oluşturulmuş) kontrol uygulanmaz.
    void OlusturanOnaylayamazKontrolEt(string? olusturanKullanici, string belgeTuru);

    // Giriş yapmış kullanıcının şubesi (login claim'inden okunur, DB'ye gitmez).
    // Kullanıcının şubesi yoksa (Admin/Muhasebe gibi merkez rolleri) null döner.
    int? MevcutKullaniciSubeId();

    // Şube bazlı erişim kontrolü: belgeSubeId null ise (eski kayıt veya merkezi
    // işlem) herkese açıktır. Admin her zaman erişebilir. Şubesi olmayan kullanıcı
    // (merkez rolü) da her şubeye erişebilir. Aksi halde kullanıcının şubesi
    // belgenin şubesiyle eşleşmelidir.
    bool SubeErisimVarMi(int? belgeSubeId);
}
