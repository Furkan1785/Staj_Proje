using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMalzemeService
{
    Task<(IEnumerable<Malzeme> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<Malzeme> CreateAsync(Malzeme malzeme);
}
