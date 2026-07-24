using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IAlisSiparisiService
{
    Task<AlisSiparisi> CreateAsync(AlisSiparisi siparis, List<AlisSiparisiKalemi> kalemler);

    Task<(IEnumerable<AlisSiparisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);
}
