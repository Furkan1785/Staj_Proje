using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ICariFisiService
{
    Task<CariFisi> CreateAsync(CariFisi fis);

    Task<(IEnumerable<CariFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);
}
