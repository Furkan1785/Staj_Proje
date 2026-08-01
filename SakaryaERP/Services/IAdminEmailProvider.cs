namespace SakaryaERP.Services;

// BildirimService'i UserManager/Identity'nin karmaşık kurulumundan ayırmak için
// küçük bir soyutlama — "alıcılar kim" (Identity konusu) ile "ne zaman/ne içerikte
// bildirim gönderilir" (iş mantığı) ayrı kalıyor, bu da BildirimService'i
// UserManager mock'lamadan test edilebilir kılıyor.
public interface IAdminEmailProvider
{
    Task<List<string>> AdminEpostalariniGetirAsync();
}
