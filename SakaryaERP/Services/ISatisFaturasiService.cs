using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ISatisFaturasiService
{
    Task<SatisFaturasi> CreateAsync(SatisFaturasi fatura, List<SatisFaturasiKalemi> kalemler);

    Task<(IEnumerable<SatisFaturasi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<SatisFaturasi?> GetByIdDetayAsync(int id);
    Task<bool> AktifFaturaVarMiIrsaliyeIcinAsync(int sevkIrsaliyesiId);
    Task<bool> AktifFaturaVarMiSiparisIcinAsync(int satisSiparisiId);

    // Satış raporları için: onaylanmış tüm faturalar (Cari + Kalemler.Malzeme ile).
    Task<List<SatisFaturasi>> GetOnaylanmisListeAsync(DateTime? baslangic = null, DateTime? bitis = null);

    // Malzeme detay sayfasındaki "son satışlar" için: tüm satış geçmişini değil, sadece bu
    // malzemeye ait kalemleri SQL'de filtreleyip en yeni N tanesini getirir.
    Task<List<SatisFaturasiKalemi>> GetMalzemeSonSatislariAsync(int malzemeId, int adet);

    // Onaylandığında: irsaliyeden gelmiyorsa stok düşülür (negatif stok kontrolüyle),
    // her durumda cariye borç hareketi (CariFisi) eklenir — tek transaction.
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);
}
