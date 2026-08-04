using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMalzemeHareketFisiService
{
    Task<MalzemeHareketFisi> CreateAsync(MalzemeHareketFisi fis, List<MalzemeHareketFisiKalemi> kalemler);

    Task<(IEnumerable<MalzemeHareketFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<MalzemeHareketFisi?> GetByIdDetayAsync(int id);

    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);

    // Sadece onaylanmış fişlerin kalemleri — bekleyen bir fiş henüz bakiyeye
    // yansımadığı için "hareket geçmişi" sayılmaz.
    Task<List<MalzemeHareketFisiKalemi>> GetMalzemeGecmisiAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId = null);
}
