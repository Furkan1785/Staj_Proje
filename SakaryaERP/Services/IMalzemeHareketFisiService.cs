using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMalzemeHareketFisiService
{
    Task<MalzemeHareketFisi> CreateAsync(MalzemeHareketFisi fis, List<MalzemeHareketFisiKalemi> kalemler);

    Task<(IEnumerable<MalzemeHareketFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task OnaylaAsync(int id);
}
