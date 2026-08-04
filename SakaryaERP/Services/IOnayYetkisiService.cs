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
}
