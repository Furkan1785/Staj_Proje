using SakaryaERP.ViewModels;

namespace SakaryaERP.Services;

public interface IMizanService
{
    // Basit mizan: HesapPlani bazında dönem içindeki toplam Borç/Alacak ve net bakiye.
    // Beyanname/resmi mizan formatı değil — sadece "hangi hesapta ne kadar hareket oldu,
    // bakiyesi ne" sorusuna cevap verir (muhasebe modülü kapsam dışı bıraktığı "tam
    // yevmiye/büyük defter raporları" maddesinin ötesine geçmez).
    Task<MizanViewModel> GetMizanAsync(DateTime? baslangic, DateTime? bitis);
}
