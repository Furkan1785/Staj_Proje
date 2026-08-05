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

    // Belge Takip ekranı için: vadesi belirtilen tarihe kadar olan (geçmiş + yaklaşan) onaylı
    // faturalar. Sadece vade tarihi bugünden ileride olmayan/eşik içindeki faturalar SQL'de filtrelenir.
    Task<List<SatisFaturasi>> GetVadesiYaklasanListesiAsync(DateTime yaklasmaSiniri);

    // Kardeks (Malzeme Geçmişi raporu) için: irsaliyesiz (doğrudan) onaylı satış faturalarındaki
    // stok düşüren kalemler — irsaliyeli faturalar hariç, o hareket zaten SevkIrsaliyesi
    // tarafında sayılıyor (aksi halde aynı stok düşüşü iki kez sayılır).
    Task<List<SatisFaturasiKalemi>> GetMalzemeDogrudanSatisHareketleriAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId);

    // Onaylandığında: irsaliyeden gelmiyorsa stok düşülür (negatif stok kontrolüyle),
    // her durumda cariye borç hareketi (CariFisi) eklenir — tek transaction.
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);
}
