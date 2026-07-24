using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMusteriTalebiService
{
    Task<MusteriTalebi> CreateAsync(MusteriTalebi talep);

    Task<(IEnumerable<MusteriTalebi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<MusteriTalebi?> GetByIdAsync(int id);

    Task IslemeAlAsync(int id);
    Task IptalEtAsync(int id);
}
