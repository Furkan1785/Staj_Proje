using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ISevkIrsaliyesiService
{
    Task<SevkIrsaliyesi> CreateAsync(SevkIrsaliyesi irsaliye, List<SevkIrsaliyesiKalemi> kalemler);

    Task<(IEnumerable<SevkIrsaliyesi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<SevkIrsaliyesi?> GetByIdDetayAsync(int id);

    // Kardeks (Malzeme Geçmişi raporu) için: bir malzemenin onaylı sevk irsaliyeleri üzerinden
    // stoktan düştüğü hareketler.
    Task<List<SevkIrsaliyesiKalemi>> GetMalzemeHareketleriAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId);

    // Onaylandığında ilgili malzemelerin stok bakiyesini düşürür (yetersiz stokta hata verir, transaction: tek SaveChangesAsync).
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);
}
